using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Galeria.Interfaces;
using TurismoEstancia.Services.Infra.Interfaces;

namespace TurismoEstancia.Services.Galeria.Services;

/// <summary>Implementação do serviço da Galeria de Estância.</summary>
public class GaleriaService : IGaleriaService
{
    private readonly AppDbContext _db;
    private readonly IArquivoService _arquivos;

    public GaleriaService(AppDbContext db, IArquivoService arquivos)
    {
        _db = db;
        _arquivos = arquivos;
    }

    private static readonly Expression<Func<GaleriaCategoria, GaleriaCategoriaDto>> ToDto =
        c => new GaleriaCategoriaDto
        {
            Id = c.Id,
            Nome = c.Nome,
            Chave = c.Chave,
            Descricao = c.Descricao,
            CapaArquivoId = c.CapaArquivoId,
            Ordem = c.Ordem,
            Ativo = c.Ativo,
            QuantidadeFotos = c.Midias.Count(m => m.Ativo)
        };

    // ---- Categorias ----

    public async Task<IReadOnlyList<GaleriaCategoriaDto>> ListarCategoriasAsync(bool incluirInativas = false, CancellationToken ct = default) =>
        await _db.GaleriaCategorias.AsNoTracking()
            .Where(c => incluirInativas || c.Ativo)
            .OrderBy(c => c.Ordem)
            .ThenBy(c => c.Nome)
            .Select(ToDto)
            .ToListAsync(ct);

    public async Task<GaleriaCategoriaDto?> ObterCategoriaPorIdAsync(int id, CancellationToken ct = default)
    {
        var dto = await _db.GaleriaCategorias.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync(ct);
        if (dto is null) return null;

        dto.Midias = await ListarFotosAsync(id, apenasAtivos: false, ct);
        return dto;
    }

    public async Task<GaleriaCategoriaDto?> ObterCategoriaPorChaveAsync(string chave, CancellationToken ct = default)
    {
        var dto = await _db.GaleriaCategorias.AsNoTracking()
            .Where(c => c.Chave == chave && c.Ativo)
            .Select(ToDto)
            .FirstOrDefaultAsync(ct);
        if (dto is null) return null;

        dto.Midias = await ListarFotosAsync(dto.Id, apenasAtivos: true, ct);
        return dto;
    }

    public async Task SalvarCategoriaAsync(GaleriaCategoriaDto dto, IFormFile? capa = null, CancellationToken ct = default)
    {
        // Chave vazia → gerada a partir do nome (ex.: "Festas e Tradições" → "festas-e-tradicoes").
        dto.Chave = string.IsNullOrWhiteSpace(dto.Chave) ? NormalizarChave(dto.Nome) : NormalizarChave(dto.Chave);

        if (await _db.GaleriaCategorias.AnyAsync(c => c.Chave == dto.Chave && c.Id != dto.Id, ct))
            throw new InvalidOperationException("Já existe uma categoria com esta chave.");

        long? capaAntigaId = null;

        if (dto.Id == 0)
        {
            var nova = new GaleriaCategoria
            {
                Nome = dto.Nome,
                Chave = dto.Chave,
                Descricao = dto.Descricao,
                Ordem = dto.Ordem,
                Ativo = dto.Ativo
            };

            if (capa is { Length: > 0 })
                nova.CapaArquivoId = await _arquivos.SalvarImagemOtimizadaAsync(capa, comMarcaDagua: true, ct: ct);

            _db.GaleriaCategorias.Add(nova);
        }
        else
        {
            var entidade = await _db.GaleriaCategorias.FirstOrDefaultAsync(c => c.Id == dto.Id, ct)
                ?? throw new InvalidOperationException("Categoria não encontrada.");
            entidade.Nome = dto.Nome;
            entidade.Chave = dto.Chave;
            entidade.Descricao = dto.Descricao;
            entidade.Ordem = dto.Ordem;
            entidade.Ativo = dto.Ativo;

            if (capa is { Length: > 0 })
            {
                capaAntigaId = entidade.CapaArquivoId;
                entidade.CapaArquivoId = await _arquivos.SalvarImagemOtimizadaAsync(capa, comMarcaDagua: true, ct: ct);
            }
        }

        await _db.SaveChangesAsync(ct);

        // Remove a capa antiga só após o commit (a checagem de referência no
        // banco impediria a exclusão enquanto a categoria ainda a apontasse).
        if (capaAntigaId.HasValue)
            await _arquivos.ExcluirAsync(capaAntigaId.Value, ct);
    }

    public async Task ExcluirCategoriaAsync(int id, CancellationToken ct = default)
    {
        var categoria = await _db.GaleriaCategorias.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new InvalidOperationException("Categoria não encontrada.");

        // Recolhe os binários ANTES do cascade remover as mídias (a checagem de
        // referência no banco impediria a exclusão se os registros ainda existissem).
        var binarios = await _db.GaleriaMidias
            .Where(m => m.CategoriaId == id)
            .Select(m => new { m.ArquivoId, m.ArquivoThumbId })
            .ToListAsync(ct);
        var capaId = categoria.CapaArquivoId;

        _db.GaleriaCategorias.Remove(categoria);
        await _db.SaveChangesAsync(ct);

        foreach (var b in binarios)
        {
            await _arquivos.ExcluirAsync(b.ArquivoId, ct);
            if (b.ArquivoThumbId.HasValue)
                await _arquivos.ExcluirAsync(b.ArquivoThumbId.Value, ct);
        }

        if (capaId.HasValue)
            await _arquivos.ExcluirAsync(capaId.Value, ct);
    }

    // ---- Fotos ----

    public async Task<IReadOnlyList<GaleriaMidiaDto>> ListarFotosAsync(int categoriaId, bool apenasAtivos = true, CancellationToken ct = default) =>
        await _db.GaleriaMidias.AsNoTracking()
            .Where(m => m.CategoriaId == categoriaId && (!apenasAtivos || m.Ativo))
            .OrderBy(m => m.Ordem)
            .Select(m => new GaleriaMidiaDto
            {
                Id = m.Id,
                CategoriaId = m.CategoriaId,
                ArquivoId = m.ArquivoId,
                ArquivoThumbId = m.ArquivoThumbId,
                Titulo = m.Titulo,
                Ordem = m.Ordem,
                Ativo = m.Ativo,
                Visualizacoes = m.Visualizacoes,
                Curtidas = m.Curtidas
            })
            .ToListAsync(ct);

    public async Task<IReadOnlyList<GaleriaMidiaDto>> ListarFotosTodasAsync(bool apenasAtivos = true, CancellationToken ct = default)
    {
        var fotos = await _db.GaleriaMidias.AsNoTracking()
            .Where(m => m.Categoria!.Ativo && (!apenasAtivos || m.Ativo))
            .OrderBy(m => m.Categoria!.Ordem)
            .ThenBy(m => m.Ordem)
            .Select(m => new GaleriaMidiaDto
            {
                Id = m.Id,
                CategoriaId = m.CategoriaId,
                CategoriaNome = m.Categoria != null ? m.Categoria.Nome : null,
                CategoriaChave = m.Categoria != null ? m.Categoria.Chave : null,
                ArquivoId = m.ArquivoId,
                ArquivoThumbId = m.ArquivoThumbId,
                Titulo = m.Titulo,
                Ordem = m.Ordem,
                Ativo = m.Ativo,
                Visualizacoes = m.Visualizacoes,
                Curtidas = m.Curtidas
            })
            .ToListAsync(ct);

        // A mesma foto pode estar em várias categorias: na visão "Todas" ela
        // aparece UMA vez (na categoria de menor Ordem), sem duplicar o grid.
        return fotos.GroupBy(f => f.ArquivoId).Select(g => g.First()).ToList();
    }

    public async Task<IReadOnlyList<GaleriaMidiaDto>> ListarFotosDisponiveisAsync(int categoriaId, CancellationToken ct = default)
    {
        var vinculadas = await _db.GaleriaMidias.AsNoTracking()
            .Where(m => m.CategoriaId == categoriaId)
            .Select(m => m.ArquivoId)
            .ToListAsync(ct);
        var emUso = vinculadas.ToHashSet();

        var fotos = await _db.GaleriaMidias.AsNoTracking()
            .Where(m => m.Categoria!.Ativo && m.Ativo)
            .OrderBy(m => m.Categoria!.Ordem)
            .ThenBy(m => m.Ordem)
            .Select(m => new GaleriaMidiaDto
            {
                Id = m.Id,
                CategoriaId = m.CategoriaId,
                CategoriaNome = m.Categoria != null ? m.Categoria.Nome : null,
                CategoriaChave = m.Categoria != null ? m.Categoria.Chave : null,
                ArquivoId = m.ArquivoId,
                ArquivoThumbId = m.ArquivoThumbId,
                Titulo = m.Titulo,
                Visualizacoes = m.Visualizacoes,
                Curtidas = m.Curtidas
            })
            .ToListAsync(ct);

        // Só fotos ainda não vinculadas à categoria; cada foto uma única vez.
        return fotos
            .Where(f => !emUso.Contains(f.ArquivoId))
            .GroupBy(f => f.ArquivoId)
            .Select(g => g.First())
            .ToList();
    }

    public async Task VincularFotosAsync(int categoriaId, IEnumerable<long> arquivoIds, CancellationToken ct = default)
    {
        if (!await _db.GaleriaCategorias.AnyAsync(c => c.Id == categoriaId, ct))
            throw new InvalidOperationException("Categoria não encontrada.");

        var ids = arquivoIds?.Distinct().ToList() ?? new List<long>();
        if (ids.Count == 0)
            throw new InvalidOperationException("Nenhuma foto selecionada.");

        // Modelo de referência de cada foto (thumbnail + legenda) a partir de um
        // vínculo existente — NENHUM binário novo é gravado (otimização mantida).
        var modelos = await _db.GaleriaMidias.AsNoTracking()
            .Where(m => ids.Contains(m.ArquivoId) && m.Ativo)
            .Select(m => new { m.ArquivoId, m.ArquivoThumbId, m.Titulo })
            .ToListAsync(ct);

        // Ignora fotos que já estão na categoria (o índice único também protege).
        var jaVinculadas = await _db.GaleriaMidias
            .Where(m => m.CategoriaId == categoriaId && ids.Contains(m.ArquivoId))
            .Select(m => m.ArquivoId)
            .ToListAsync(ct);
        var ja = jaVinculadas.ToHashSet();

        var ordemBase = await _db.GaleriaMidias.CountAsync(m => m.CategoriaId == categoriaId, ct);
        var indice = 0;
        foreach (var modelo in modelos)
        {
            if (ja.Contains(modelo.ArquivoId))
                continue;

            _db.GaleriaMidias.Add(new GaleriaMidia
            {
                CategoriaId = categoriaId,
                ArquivoId = modelo.ArquivoId,
                ArquivoThumbId = modelo.ArquivoThumbId,
                Titulo = modelo.Titulo,
                Ordem = ordemBase + (++indice),
                Ativo = true
            });
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task AdicionarFotosAsync(int categoriaId, IEnumerable<IFormFile> fotos, CancellationToken ct = default)
    {
        if (!await _db.GaleriaCategorias.AnyAsync(c => c.Id == categoriaId, ct))
            throw new InvalidOperationException("Categoria não encontrada.");

        var pendentes = fotos.Where(f => f.Length > 0).ToList();
        if (pendentes.Count == 0)
            throw new InvalidOperationException("Nenhuma imagem selecionada.");

        var ordemBase = await _db.GaleriaMidias.CountAsync(m => m.CategoriaId == categoriaId, ct);

        // Se qualquer foto falhar (formato inválido, etc.), remove os binários já
        // gravados — as mídias nem chegaram ao banco, então nada fica órfão.
        var criados = new List<long>();
        try
        {
            var indice = 0;
            foreach (var foto in pendentes)
            {
                // Imagem cheia com MARCA D'ÁGUA (proteção contra download) e
                // thumbnail sem marca — ambos otimizados (1600px / 400px).
                var arquivoId = await _arquivos.SalvarImagemOtimizadaAsync(foto, comMarcaDagua: true, ct: ct);
                criados.Add(arquivoId);

                var thumbId = await _arquivos.SalvarThumbnailAsync(foto, ct: ct);
                criados.Add(thumbId);

                _db.GaleriaMidias.Add(new GaleriaMidia
                {
                    CategoriaId = categoriaId,
                    ArquivoId = arquivoId,
                    ArquivoThumbId = thumbId,
                    Ordem = ordemBase + (++indice),
                    Ativo = true
                });
            }

            await _db.SaveChangesAsync(ct);
        }
        catch
        {
            foreach (var id in criados)
                await _arquivos.ExcluirAsync(id, ct);
            throw;
        }
    }

    public async Task AtualizarFotoAsync(GaleriaMidiaDto dto, CancellationToken ct = default)
    {
        var midia = await _db.GaleriaMidias.FirstOrDefaultAsync(m => m.Id == dto.Id, ct)
            ?? throw new InvalidOperationException("Foto não encontrada.");
        midia.Titulo = dto.Titulo;
        midia.Ativo = dto.Ativo;
        await _db.SaveChangesAsync(ct);
    }

    public async Task MoverFotoAsync(int id, int direcao, CancellationToken ct = default)
    {
        var midia = await _db.GaleriaMidias.FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new InvalidOperationException("Foto não encontrada.");

        // Vizinho na direção pedida: o de Ordem imediatamente menor (subir) ou
        // maior (descer). Já é a primeira/última → sem vizinho → nada a fazer.
        GaleriaMidia? vizinho;
        if (direcao < 0)
        {
            vizinho = await _db.GaleriaMidias
                .Where(m => m.CategoriaId == midia.CategoriaId && m.Ordem < midia.Ordem)
                .OrderByDescending(m => m.Ordem)
                .FirstOrDefaultAsync(ct);
        }
        else
        {
            vizinho = await _db.GaleriaMidias
                .Where(m => m.CategoriaId == midia.CategoriaId && m.Ordem > midia.Ordem)
                .OrderBy(m => m.Ordem)
                .FirstOrDefaultAsync(ct);
        }

        if (vizinho is null)
            return;

        (midia.Ordem, vizinho.Ordem) = (vizinho.Ordem, midia.Ordem);
        await _db.SaveChangesAsync(ct);
    }

    public async Task ExcluirFotoAsync(int id, CancellationToken ct = default)
    {
        var midia = await _db.GaleriaMidias.FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new InvalidOperationException("Foto não encontrada.");

        var arquivoId = midia.ArquivoId;
        var thumbId = midia.ArquivoThumbId;

        _db.GaleriaMidias.Remove(midia);
        await _db.SaveChangesAsync(ct);

        await _arquivos.ExcluirAsync(arquivoId, ct);
        if (thumbId.HasValue)
            await _arquivos.ExcluirAsync(thumbId.Value, ct);
    }

    // ---- Engajamento (visualizações e curtidas) ----

    public async Task<int> RegistrarVisualizacaoAsync(int id, CancellationToken ct = default)
    {
        var midia = await _db.GaleriaMidias.FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new InvalidOperationException("Foto não encontrada.");
        midia.Visualizacoes++;
        await _db.SaveChangesAsync(ct);
        return midia.Visualizacoes;
    }

    public async Task<GaleriaCurtidaResultado> CurtirAsync(int id, string sessaoId, CancellationToken ct = default)
    {
        var midia = await _db.GaleriaMidias.FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new InvalidOperationException("Foto não encontrada.");

        // Dedup anônimo (LGPD): a sessão já curtiu esta foto antes?
        var jaCurtiu = await _db.AnalyticsEventos.AsNoTracking().AnyAsync(
            e => e.Tipo == "Clique" && e.Evento == "like-foto" && e.EntidadeId == id && e.SessaoId == sessaoId, ct);

        var resultado = new GaleriaCurtidaResultado
        {
            Curtidas = midia.Curtidas,
            JaCurtiu = jaCurtiu,
            Titulo = midia.Titulo
        };

        if (!jaCurtiu)
        {
            midia.Curtidas++;
            await _db.SaveChangesAsync(ct);
            resultado.Curtidas = midia.Curtidas;
            resultado.Curtiu = true;
        }

        return resultado;
    }

    /// <summary>Gera a chave/slug a partir de um texto (ex.: "Praia do Saco" → "praia-do-saco").</summary>
    private static string NormalizarChave(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return "";

        var normalizado = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalizado.Length);
        foreach (var ch in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }

        var limpo = sb.ToString()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant()
            .Replace(' ', '-');

        var result = new StringBuilder(limpo.Length);
        var ultimoHifen = false;
        foreach (var ch in limpo)
        {
            if (char.IsAsciiLetterOrDigit(ch))
            {
                result.Append(ch);
                ultimoHifen = false;
            }
            else if (!ultimoHifen)
            {
                result.Append('-');
                ultimoHifen = true;
            }
        }

        return result.ToString().Trim('-');
    }
}
