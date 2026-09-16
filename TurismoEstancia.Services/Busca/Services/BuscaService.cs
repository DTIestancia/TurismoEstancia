using System.Globalization;
using TurismoEstancia.Services.Busca.Interfaces;
using TurismoEstancia.Services.Comunicacao.Interfaces;
using TurismoEstancia.Services.ConhecaEstancia.Interfaces;
using TurismoEstancia.Services.CulturaGastronomia.Interfaces;
using TurismoEstancia.Services.Planeje.Interfaces;
using TurismoEstancia.Services.Turismo.Interfaces;
using TurismoEstancia.Domain.DTOs;

namespace TurismoEstancia.Services.Busca.Services;

/// <summary>
/// Implementação da busca pública: carrega os conteúdos ativos e filtra em
/// memória com comparação pt-BR sem acento (o volume é pequeno e o LIKE do
/// banco não garantiria o folding em todas as collations).
/// </summary>
public class BuscaService : IBuscaService
{
    private const int MaxPorGrupo = 6;

    private static readonly CompareInfo Comparador =
        CultureInfo.GetCultureInfo("pt-BR").CompareInfo;

    private const CompareOptions Opcoes =
        CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

    private readonly IPontoTuristicoService _pontos;
    private readonly IPlanejeService _planeje;
    private readonly INoticiaService _noticias;
    private readonly IEventoService _eventos;
    private readonly IConhecaEstanciaService _conheca;
    private readonly IPratoTuristicoService _pratos;
    private readonly IGrupoCulturalService _grupos;

    public BuscaService(
        IPontoTuristicoService pontos,
        IPlanejeService planeje,
        INoticiaService noticias,
        IEventoService eventos,
        IConhecaEstanciaService conheca,
        IPratoTuristicoService pratos,
        IGrupoCulturalService grupos)
    {
        _pontos = pontos;
        _planeje = planeje;
        _noticias = noticias;
        _eventos = eventos;
        _conheca = conheca;
        _pratos = pratos;
        _grupos = grupos;
    }

    public async Task<BuscaResultadoDto> BuscarAsync(string? termo, CancellationToken ct = default)
    {
        var vm = new BuscaResultadoDto { Termo = (termo ?? "").Trim() };
        var termos = vm.Termo.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => t.Length >= 2)
            .ToArray();
        if (termos.Length == 0)
            return vm;

        var grupos = new List<GrupoBuscaDto>();

        var pontos = await _pontos.ListarAsync(true, ct);
        grupos.Add(Grupo("7 Maravilhas", "gem", pontos
            .Where(p => termos.All(t => Contem(p.Nome, p.Descricao, p.CategoriaNome, t)))
            .Take(MaxPorGrupo)
            .Select(p => new ItemBuscaDto
            {
                Titulo = p.Nome,
                Resumo = p.Descricao,
                Url = $"/lugares/{p.Id}",
                Rotulo = p.CategoriaNome,
                ImagemArquivoId = p.CapaArquivoId
            })));

        var planeje = await _planeje.ListarItensAsync(true, ct);
        grupos.Add(Grupo("Planeje sua viagem", "route", planeje
            .Where(i => termos.All(t => Contem(i.Titulo, $"{i.Descricao} {i.Localizacao} {i.CategoriaNome}", null, t)))
            .Take(MaxPorGrupo)
            .Select(i => new ItemBuscaDto
            {
                Titulo = i.Titulo,
                Resumo = i.Descricao,
                Url = "/roteiros",
                Rotulo = i.CategoriaNome,
                ImagemArquivoId = i.ImagemArquivoId ?? i.CategoriaImagemPadraoArquivoId
            })));

        var noticias = await _noticias.ListarAsync(apenasPublicadas: true, ct);
        grupos.Add(Grupo("Notícias", "newspaper", noticias
            .Where(n => termos.All(t => Contem(n.Titulo, n.Resumo, null, t)))
            .Take(MaxPorGrupo)
            .Select(n => new ItemBuscaDto
            {
                Titulo = n.Titulo,
                Resumo = n.Resumo,
                Url = $"/Noticias/Detalhe/{n.Slug}",
                ImagemArquivoId = n.ImagemArquivoId
            })));

        var eventos = await _eventos.ListarAsync(apenasProximos: false, ct);
        grupos.Add(Grupo("Agenda", "calendar", eventos
            .Where(e => e.Ativo && termos.All(t => Contem(e.Titulo, e.Descricao, e.Local, t)))
            .Take(MaxPorGrupo)
            .Select(e => new ItemBuscaDto
            {
                Titulo = e.Titulo,
                Resumo = e.Descricao,
                Url = "/agenda",
                Rotulo = e.DataInicio.ToString("dd/MM/yyyy")
            })));

        var conheca = await _conheca.ListarAtivosAsync(ct);
        grupos.Add(Grupo("Conheça Estância", "compass", conheca
            .Where(i => termos.All(t => Contem(i.Nome, i.Descricao, null, t)))
            .Take(MaxPorGrupo)
            .Select(i => new ItemBuscaDto
            {
                Titulo = i.Nome,
                Resumo = i.Descricao,
                Url = $"/conheca-estancia/{i.Id}",
                Rotulo = i.Categoria.ToString(),
                ImagemArquivoId = i.ImagemArquivoId
            })));

        var pratos = await _pratos.ListarAsync(ct);
        grupos.Add(Grupo("Gastronomia", "utensils", pratos
            .Where(p => p.Ativo && termos.All(t => Contem(p.Nome, p.Descricao, null, t)))
            .Take(MaxPorGrupo)
            .Select(p => new ItemBuscaDto
            {
                Titulo = p.Nome,
                Resumo = p.Descricao,
                Url = "/gastronomia",
                ImagemArquivoId = p.ImagemArquivoId
            })));

        var gruposCult = await _grupos.ListarAsync(ct);
        grupos.Add(Grupo("Grupos populares", "music", gruposCult
            .Where(g => termos.All(t => Contem(g.Nome, g.Descricao, null, t)))
            .Take(MaxPorGrupo)
            .Select(g => new ItemBuscaDto
            {
                Titulo = g.Nome,
                Resumo = g.Descricao,
                Url = "/grupos-populares",
                ImagemArquivoId = g.ImagemArquivoId
            })));

        vm.Grupos = grupos.Where(g => g.Itens.Count > 0).ToList();
        vm.Total = vm.Grupos.Sum(g => g.Itens.Count);
        return vm;

        static GrupoBuscaDto Grupo(string titulo, string icone, IEnumerable<ItemBuscaDto> itens) => new()
        {
            Titulo = titulo,
            Icone = icone,
            Itens = itens.ToList()
        };
    }

    /// <summary>Todas as palavras aparecem em algum dos campos (título, descrição, extra).</summary>
    private static bool Contem(string? titulo, string? descricao, string? extra, string termo)
    {
        var texto = $"{titulo} {descricao} {extra}";
        return Comparador.IndexOf(texto, termo, Opcoes) >= 0;
    }
}
