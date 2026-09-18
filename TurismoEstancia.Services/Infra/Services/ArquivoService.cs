using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Infra.Arquivos;
using TurismoEstancia.Services.Infra.Imagens;
using TurismoEstancia.Services.Infra.Interfaces;

namespace TurismoEstancia.Services.Infra.Services;

/// <summary>Implementação do serviço de Arquivo.</summary>
public class ArquivoService : IArquivoService
{
    /// <summary>Acha um id de arquivo citado dentro de um texto ("12" ou "/arquivo/12").</summary>
    private static readonly Regex IdCitado = new(@"/arquivo/(\d+)", RegexOptions.Compiled);

    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public ArquivoService(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public async Task<long> SalvarAsync(IFormFile arquivo, CancellationToken ct = default)
    {
        if (arquivo is null || arquivo.Length == 0)
            throw new InvalidOperationException("O arquivo está vazio.");

        LimitesDeUpload.Validar(arquivo);

        var bytes = await LerBytesAsync(arquivo, ct);

        // Toda foto enviada por qualquer tela do CMS entra otimizada — mesma regra
        // da galeria (ver OtimizadorDeImagem). O que não for foto decodificável
        // (vídeo, PDF, SVG, .ico, GIF animado) segue exatamente como veio.
        var otimizada = await OtimizadorDeImagem.OtimizarAsync(bytes, arquivo.ContentType, ct: ct);

        return otimizada is null
            ? await SalvarBytesAsync(arquivo.FileName, arquivo.ContentType, bytes, ct)
            : await SalvarBytesAsync(
                OtimizadorDeImagem.NomeComExtensao(arquivo.FileName, otimizada.Extensao),
                otimizada.ContentType, otimizada.Bytes, ct);
    }

    public async Task<long> SalvarImagemOtimizadaAsync(IFormFile arquivo, int maxDimensao = OtimizadorDeImagem.MaxDimensaoPadrao, int qualidade = OtimizadorDeImagem.QualidadeJpegPadrao, bool comMarcaDagua = false, CancellationToken ct = default)
    {
        LimitesDeUpload.Validar(arquivo);

        var bytes = await LerBytesAsync(arquivo, ct);
        var logotipo = comMarcaDagua ? await ObterLogotipoMarcaDaguaAsync(ct) : null;

        var otimizada = await OtimizadorDeImagem.OtimizarAsync(bytes, arquivo.ContentType, maxDimensao, qualidade, logotipo, ct)
            ?? throw new InvalidOperationException("Formato não suportado: envie uma imagem JPG, PNG ou WebP.");

        return await SalvarBytesAsync(
            OtimizadorDeImagem.NomeComExtensao(arquivo.FileName, otimizada.Extensao),
            otimizada.ContentType, otimizada.Bytes, ct);
    }

    public async Task<long> SalvarThumbnailAsync(IFormFile arquivo, int maxDimensao = 400, int qualidade = 75, CancellationToken ct = default)
    {
        LimitesDeUpload.Validar(arquivo);

        var bytes = await LerBytesAsync(arquivo, ct);
        var otimizada = await OtimizadorDeImagem.OtimizarAsync(bytes, arquivo.ContentType, maxDimensao, qualidade, ct: ct)
            ?? throw new InvalidOperationException("Formato não suportado: envie uma imagem JPG, PNG ou WebP.");

        return await SalvarBytesAsync(
            OtimizadorDeImagem.NomeComExtensao(arquivo.FileName, otimizada.Extensao),
            otimizada.ContentType, otimizada.Bytes, ct);
    }

    /// <summary>Lê o upload inteiro para memória — os bytes vão para o banco.</summary>
    private static async Task<byte[]> LerBytesAsync(IFormFile arquivo, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await arquivo.CopyToAsync(ms, ct);
        return ms.ToArray();
    }

    /// <summary>
    /// Logotipo do portal (configuração "logo-principal") usado na marca d'água da
    /// galeria. Vem daqui, e não do otimizador, para a regra de imagem continuar
    /// sem depender de banco.
    /// </summary>
    private async Task<byte[]?> ObterLogotipoMarcaDaguaAsync(CancellationToken ct) =>
        await _db.ConfiguracoesSite.AsNoTracking()
            .Where(c => c.Chave == "logo-principal" && c.ArquivoId != null)
            .Select(c => c.Arquivo!.Bytes)
            .FirstOrDefaultAsync(ct);

    public async Task<long> SalvarFaviconAsync(IFormFile arquivo, int dimensao = 64, CancellationToken ct = default)
    {
        LimitesDeUpload.Validar(arquivo);

        try
        {
            await using var origem = arquivo.OpenReadStream();
            using var imagem = await Image.LoadAsync(origem, ct);

            // Redimensiona só para reduzir (nunca amplia), encaixando em um
            // quadrado dimensao×dimensao — PNG quadrado de fonte vira 64×64.
            if (imagem.Width > dimensao || imagem.Height > dimensao)
            {
                imagem.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(dimensao, dimensao)
                }));
            }

            using var ms = new MemoryStream();
            await imagem.SaveAsync(ms, new PngEncoder { SkipMetadata = true }, ct);
            return await SalvarBytesAsync(arquivo.FileName, "image/png", ms.ToArray(), ct);
        }
        catch (OperationCanceledException) { throw; }
        catch
        {
            // Não-imagem (ex.: .ico) ou decode falhou: salva como está — o
            // favicon nunca deve travar por causa de um formato inesperado.
            return await SalvarAsync(arquivo, ct);
        }
    }

    public async Task<long> SalvarBytesAsync(string nome, string contentType, byte[] bytes, CancellationToken ct = default)
    {
        var novo = new Arquivo
        {
            UID = Guid.NewGuid(),
            Nome = nome,
            ContentType = contentType,
            Size = bytes.LongLength,
            Bytes = bytes,
            // Padrão Arqu*: autor = usuário logado; origem = canal que gravou.
            Autor = _http.HttpContext?.User?.Identity?.Name,
            Origem = _http.HttpContext is not null ? "cms" : null,
            Ativo = true
        };

        _db.Arquivos.Add(novo);
        await _db.SaveChangesAsync(ct);
        return novo.Id;
    }

    public async Task<Arquivo> ObterAsync(long id, CancellationToken ct = default)
    {
        var arquivo = await _db.Arquivos.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new InvalidOperationException("Arquivo não encontrado.");
        return arquivo;
    }

    /// <summary>
    /// Abre o binário para leitura direto do banco, junto com o tipo e o tamanho
    /// <b>real</b> do blob (<c>DATALENGTH</c>, e não a coluna <c>Size</c>: o Range do
    /// ASP.NET se apoia nesse número, então ele precisa ser o do binário).
    ///
    /// O fluxo devolvido não é materializado nem copiado para lugar nenhum — cada
    /// janela é buscada do servidor conforme a leitura avança (ver
    /// <see cref="FluxoDoArquivoNoBanco"/>). Devolve <c>null</c> quando o id não existe.
    /// </summary>
    public async Task<(Stream Fluxo, MetadadosDeArquivo Metadados)?> AbrirAsync(long id, CancellationToken ct = default)
    {
        var conexao = _db.Database.GetDbConnection();
        var abriuAgora = conexao.State != ConnectionState.Open;
        if (abriuAgora)
            await conexao.OpenAsync(ct);

        try
        {
            using var comando = conexao.CreateCommand();
            comando.CommandText =
                "SELECT ArquContentType, DATALENGTH(ArquBytes), ArquMomento FROM Arquivos WHERE ArquId = @id";

            var parametro = comando.CreateParameter();
            parametro.ParameterName = "@id";
            parametro.Value = id;
            comando.Parameters.Add(parametro);

            using var leitor = await comando.ExecuteReaderAsync(ct);
            if (!await leitor.ReadAsync(ct))
                return null;

            var contentType = leitor.GetString(0);
            var tamanho = leitor.IsDBNull(1) ? 0L : leitor.GetInt64(1);
            var criadoEm = leitor.GetDateTime(2);

            return (new FluxoDoArquivoNoBanco(_db, id, tamanho), new MetadadosDeArquivo(contentType, tamanho, criadoEm));
        }
        finally
        {
            if (abriuAgora)
                await conexao.CloseAsync();
        }
    }

    public async Task<byte[]?> GerarPngRedimensionadoAsync(long arquivoId, int maxDimensao, CancellationToken ct = default)
    {
        try
        {
            var arquivo = await _db.Arquivos.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == arquivoId, ct);
            if (arquivo?.Bytes is not { Length: > 0 })
                return null;

            using var imagem = await Image.LoadAsync(new MemoryStream(arquivo.Bytes), ct);
            imagem.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(maxDimensao, maxDimensao)
            }));

            using var ms = new MemoryStream();
            await imagem.SaveAsync(ms, new PngEncoder { SkipMetadata = true }, ct);
            return ms.ToArray();
        }
        catch (OperationCanceledException) { throw; }
        catch
        {
            // Não-imagem (ex.: .ico): impossível derivar um PNG — sinaliza com null.
            return null;
        }
    }

    public async Task<(byte[] Bytes, string ContentType, string Extensao)?> GerarRedimensionadoAsync(long arquivoId, int larguraMaxima, CancellationToken ct = default)
    {
        // Só os bytes importam: a projeção evita trazer o registro inteiro.
        var arquivo = await _db.Arquivos.AsNoTracking()
            .Where(a => a.Id == arquivoId)
            .Select(a => new { a.Bytes, a.ContentType })
            .FirstOrDefaultAsync(ct);

        if (arquivo?.Bytes is not { Length: > 0 })
            return null;

        // Mesma regra de imagem do upload (ver OtimizadorDeImagem): aqui não existe mais
        // resize/encode próprio. A versão antiga duplicava a regra e mantinha o EXIF no
        // arquivo derivado — ou seja, o GPS da foto viajava em cada miniatura servida com
        // cache de um ano, e os pixels saíam sem a rotação aplicada, deixando a orientação
        // na mão de quem for consumir a imagem.
        var reduzida = await OtimizadorDeImagem.ReduzirAsync(
            arquivo.Bytes, arquivo.ContentType, larguraMaxima, ct: ct);

        return reduzida is null
            ? null
            : (reduzida.Bytes, reduzida.ContentType, reduzida.Extensao);
    }

    public async Task ExcluirAsync(long id, CancellationToken ct = default)
    {
        if (await EstaReferenciadoAsync(id, ct))
            return; // nunca apagar arquivo ainda referenciado

        var arquivo = await _db.Arquivos.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (arquivo is not null)
        {
            _db.Arquivos.Remove(arquivo);
            await _db.SaveChangesAsync(ct);
        }
    }

    /// <summary>
    /// Todas as colunas do sistema que apontam para um arquivo, como fontes de
    /// <c>long</c>. É a <b>lista única</b> da regra: tanto a checagem de um id
    /// (exclusão) quanto o levantamento completo (relatório de órfãos) saem daqui —
    /// acrescentar um novo vínculo no sistema é acrescentar uma linha aqui.
    /// </summary>
    private IEnumerable<IQueryable<long>> FontesDeId()
    {
        yield return _db.Slides.Select(s => s.ImagemArquivoId);
        yield return _db.PontoTuristicoMidias.Select(m => m.ArquivoId);
        yield return _db.GaleriaMidias.Select(m => m.ArquivoId);
        yield return _db.GaleriaMidias.Where(m => m.ArquivoThumbId != null).Select(m => m.ArquivoThumbId!.Value);
        yield return _db.Noticias.Where(n => n.ImagemArquivoId != null).Select(n => n.ImagemArquivoId!.Value);
        yield return _db.Roteiros.Where(r => r.ImagemArquivoId != null).Select(r => r.ImagemArquivoId!.Value);
        yield return _db.ConfiguracoesSite.Where(c => c.ArquivoId != null).Select(c => c.ArquivoId!.Value);
        yield return _db.GaleriaCategorias.Where(c => c.CapaArquivoId != null).Select(c => c.CapaArquivoId!.Value);
        yield return _db.ConhecaEstanciaItens.Where(i => i.ImagemArquivoId != null).Select(i => i.ImagemArquivoId!.Value);
        yield return _db.PratosTuristicos.Where(p => p.ImagemArquivoId != null).Select(p => p.ImagemArquivoId!.Value);
        yield return _db.GruposCulturais.Where(g => g.ImagemArquivoId != null).Select(g => g.ImagemArquivoId!.Value);
        yield return _db.TagsCulturais.Where(t => t.ImagemArquivoId != null).Select(t => t.ImagemArquivoId!.Value);
        yield return _db.PlanejeItens.Where(i => i.ImagemArquivoId != null).Select(i => i.ImagemArquivoId!.Value);
        yield return _db.PlanejeCategorias.Where(c => c.ImagemPadraoArquivoId != null).Select(c => c.ImagemPadraoArquivoId!.Value);
        yield return _db.MidiaKitItens.Where(i => i.ArquivoId != null).Select(i => i.ArquivoId!.Value);
        yield return _db.PontosTuristicos.Where(p => p.IconeArquivoId != null).Select(p => p.IconeArquivoId!.Value);
        yield return _db.CategoriasPontosTuristicos.Where(c => c.IconeArquivoId != null).Select(c => c.IconeArquivoId!.Value);
    }

    public async Task<bool> EstaReferenciadoAsync(long id, CancellationToken ct = default)
    {
        foreach (var fonte in FontesDeId())
        {
            if (await fonte.AnyAsync(x => x == id, ct))
                return true;
        }

        return await TextoApontaParaAsync(id, ct);
    }

    /// <summary>
    /// Todos os ids de arquivo citados em algum lugar do sistema. Para o relatório de
    /// órfãos, uma consulta por fonte (e não uma por arquivo).
    /// </summary>
    public async Task<IReadOnlySet<long>> IdsReferenciadosAsync(CancellationToken ct = default)
    {
        var ids = new HashSet<long>();

        foreach (var fonte in FontesDeId())
        {
            foreach (var id in await fonte.Distinct().ToListAsync(ct))
                ids.Add(id);
        }

        foreach (var texto in await TextosDeConteudoAsync(ct))
            ids.UnionWith(IdsCitados(texto));

        return ids;
    }

    /// <summary>
    /// Resumo de todo o acervo (nome, tipo, tamanho, data) <b>sem os bytes</b> — é o
    /// que o relatório de órfãos precisa para somar o que está sobrando.
    /// </summary>
    public async Task<IReadOnlyList<ResumoDeArquivo>> ListarResumoAsync(CancellationToken ct = default) =>
        await _db.Arquivos.AsNoTracking()
            .OrderBy(a => a.Id)
            .Select(a => new ResumoDeArquivo(a.Id, a.Nome, a.ContentType, a.Size, a.CriadoEm, a.Autor))
            .ToListAsync(ct);

    /// <summary>
    /// Os textos de seção que podem guardar o id de uma imagem: as seções da home
    /// gravam o arquivo como <i>texto</i> na chave (ex.: <c>historia-imagem</c> = "42"),
    /// então não existe chave estrangeira para o banco proteger.
    /// </summary>
    private async Task<List<string>> TextosDeConteudoAsync(CancellationToken ct) =>
        await _db.ConteudosSite.AsNoTracking()
            .Where(c => c.Texto != null && c.Texto != "")
            .Select(c => c.Texto!)
            .ToListAsync(ct);

    /// <summary>
    /// Um texto de seção aponta para este arquivo? Aceita o id puro ("42") e a
    /// citação em URL ("/arquivo/42?largura=800"), com verificação em memória para
    /// "42" não casar com "421".
    /// </summary>
    private async Task<bool> TextoApontaParaAsync(long id, CancellationToken ct)
    {
        var alvo = id.ToString();
        var candidatos = await _db.ConteudosSite.AsNoTracking()
            .Where(c => c.Texto == alvo || c.Texto!.Contains("/arquivo/" + alvo))
            .Select(c => c.Texto!)
            .ToListAsync(ct);

        return candidatos.Any(t => IdsCitados(t).Contains(id));
    }

    /// <summary>Ids citados num texto: o valor inteiro ou cada <c>/arquivo/{id}</c>.</summary>
    private static IEnumerable<long> IdsCitados(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            yield break;

        var limpo = texto.Trim();
        if (long.TryParse(limpo, out var direto))
            yield return direto;

        foreach (Match achado in IdCitado.Matches(limpo))
        {
            if (long.TryParse(achado.Groups[1].Value, out var citado))
                yield return citado;
        }
    }
}
