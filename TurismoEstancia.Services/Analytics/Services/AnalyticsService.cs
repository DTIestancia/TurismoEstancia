using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Analytics.Interfaces;

namespace TurismoEstancia.Services.Analytics.Services;

/// <summary>Implementação do serviço de analytics.</summary>
public class AnalyticsService : IAnalyticsService
{
    private readonly Channel<AnalyticsEvento> _canal;
    private readonly AppDbContext _db;

    public AnalyticsService(Channel<AnalyticsEvento> canal, AppDbContext db)
    {
        _canal = canal;
        _db = db;
    }

    public void Registrar(AnalyticsEventoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Rota) || string.IsNullOrWhiteSpace(dto.SessaoId))
            return;

        // DropWrite: se a fila estiver cheia, descarta (o request nunca espera).
        // Valores cortados no limite da coluna: um campo longo demais faria o
        // SaveChanges em lote falhar inteiro (e perder todos os eventos da rajada).
        _canal.Writer.TryWrite(new AnalyticsEvento
        {
            Data = dto.Data,
            Tipo = Curto(dto.Tipo, 20) ?? "Visita",
            Rota = Curto(dto.Rota, 300) ?? "",
            Titulo = Curto(dto.Titulo, 200),
            RefererHost = Curto(dto.RefererHost, 150),
            SessaoId = Curto(dto.SessaoId, 50) ?? "",
            Dispositivo = Curto(dto.Dispositivo, 20) ?? "Desktop",
            Evento = Curto(dto.Evento, 50),
            EntidadeId = dto.EntidadeId,
            EntidadeNome = Curto(dto.EntidadeNome, 150)
        });
    }

    /// <summary>Corta o texto no limite da coluna (null preservado).</summary>
    private static string? Curto(string? valor, int max)
        => string.IsNullOrEmpty(valor) ? valor : valor.Length <= max ? valor : valor[..max];

    public async Task<AnalyticsResumoDto> ObterResumoAsync(DateTime de, DateTime ate, int? galeriaCategoriaId = null, CancellationToken ct = default)
    {
        var fim = ate.Date.AddDays(1);
        var visitas = _db.AnalyticsEventos.AsNoTracking()
            .Where(e => e.Data >= de && e.Data < fim && e.Tipo == "Visita");

        var resumo = new AnalyticsResumoDto
        {
            Visitas = await visitas.CountAsync(ct),
            VisitantesUnicos = await visitas.Select(e => e.SessaoId).Distinct().CountAsync(ct),
            Cliques = await _db.AnalyticsEventos.AsNoTracking()
                .CountAsync(e => e.Tipo == "Clique" && e.Data >= de && e.Data < fim, ct),
            VisitasHoje = await _db.AnalyticsEventos.AsNoTracking()
                .CountAsync(e => e.Tipo == "Visita" && e.Data >= DateTime.Today, ct),
            VisitasPorDia = await visitas
                .GroupBy(e => e.Data.Date)
                .Select(g => new AnalyticsSerieDiaDto { Data = g.Key, Quantidade = g.Count() })
                .OrderBy(x => x.Data)
                .ToListAsync(ct),
            TopPaginas = await visitas
                .GroupBy(e => e.Rota)
                .Select(g => new AnalyticsContagemDto { Rotulo = g.Key, Quantidade = g.Count() })
                .OrderByDescending(x => x.Quantidade)
                .Take(10)
                .ToListAsync(ct),
            Dispositivos = await visitas
                .GroupBy(e => e.Dispositivo)
                .Select(g => new AnalyticsContagemDto { Rotulo = g.Key, Quantidade = g.Count() })
                .OrderByDescending(x => x.Quantidade)
                .ToListAsync(ct),
            TopMaravilhas = await _db.AnalyticsEventos.AsNoTracking()
                .Where(e => e.Tipo == "Clique" && e.Evento == "ver-maravilha" && e.Data >= de && e.Data < fim)
                .GroupBy(e => e.EntidadeNome ?? "Desconhecida")
                .Select(g => new AnalyticsContagemDto { Rotulo = g.Key, Quantidade = g.Count() })
                .OrderByDescending(x => x.Quantidade)
                .Take(7)
                .ToListAsync(ct),
            TopEventos = await _db.AnalyticsEventos.AsNoTracking()
                .Where(e => e.Tipo == "Clique" && e.Data >= de && e.Data < fim)
                .GroupBy(e => e.Evento ?? "outro")
                .Select(g => new AnalyticsContagemDto { Rotulo = g.Key, Quantidade = g.Count() })
                .OrderByDescending(x => x.Quantidade)
                .Take(8)
                .ToListAsync(ct),
            TopFotosVistas = await FotosMaisEngajadasAsync("visualizacao-foto", de, fim, galeriaCategoriaId, ct),
            TopFotosCurtidas = await FotosMaisEngajadasAsync("like-foto", de, fim, galeriaCategoriaId, ct)
        };

        await ClassificarFontesAsync(resumo, visitas, ct);
        return resumo;
    }

    /// <summary>Ranking de fotos da galeria por um evento de engajamento (visualização ou curtida).</summary>
    private async Task<List<AnalyticsContagemDto>> FotosMaisEngajadasAsync(string evento, DateTime de, DateTime fim, int? categoriaId, CancellationToken ct)
    {
        var eventos = _db.AnalyticsEventos.AsNoTracking()
            .Where(e => e.Tipo == "Clique" && e.Evento == evento && e.EntidadeId != null && e.Data >= de && e.Data < fim);

        // Filtro por categoria: o evento guarda o Id do vínculo GaleriaMidia, então
        // junta com a tabela de vínculo para restringir à categoria selecionada.
        if (categoriaId is int cid)
        {
            eventos = eventos.Where(e => _db.GaleriaMidias.Any(gm => gm.Id == e.EntidadeId && gm.CategoriaId == cid));
        }

        var ranking = await eventos
            .GroupBy(e => new { e.EntidadeId, e.EntidadeNome })
            .Select(g => new
            {
                g.Key.EntidadeId,
                g.Key.EntidadeNome,
                Quantidade = g.Count()
            })
            .OrderByDescending(x => x.Quantidade)
            .Take(10)
            .ToListAsync(ct);

        return ranking
            .Select(r => new AnalyticsContagemDto
            {
                Rotulo = string.IsNullOrWhiteSpace(r.EntidadeNome) ? $"Foto #{r.EntidadeId}" : r.EntidadeNome!,
                Quantidade = r.Quantidade
            })
            .ToList();
    }

    /// <summary>Busca os referrers e classifica em Buscas/Redes sociais/Direto/Outros.</summary>
    private async Task ClassificarFontesAsync(AnalyticsResumoDto resumo, IQueryable<AnalyticsEvento> visitas, CancellationToken ct)
    {
        var porHost = await visitas
            .Where(e => e.RefererHost != null && e.RefererHost != "")
            .GroupBy(e => e.RefererHost!)
            .Select(g => new { Host = g.Key, N = g.Count() })
            .ToListAsync(ct);

        var contagem = new Dictionary<string, int>
        {
            ["Buscas"] = 0,
            ["Redes sociais"] = 0,
            ["Outros sites"] = 0,
            ["Acesso direto"] = 0
        };

        foreach (var item in porHost)
        {
            var fonte = ClassificarHost(item.Host);
            contagem[fonte] = contagem.GetValueOrDefault(fonte) + item.N;
        }

        var direto = (int)resumo.Visitas - porHost.Sum(p => p.N);
        if (direto > 0)
            contagem["Acesso direto"] = contagem.GetValueOrDefault("Acesso direto") + direto;

        resumo.Fontes = contagem
            .Where(kv => kv.Value > 0)
            .Select(kv => new AnalyticsContagemDto { Rotulo = kv.Key, Quantidade = kv.Value })
            .OrderByDescending(x => x.Quantidade)
            .ToList();

        resumo.TopReferrers = porHost
            .OrderByDescending(p => p.N)
            .Take(8)
            .Select(p => new AnalyticsContagemDto { Rotulo = p.Host, Quantidade = p.N })
            .ToList();
    }

    private static string ClassificarHost(string host)
    {
        var h = host.ToLowerInvariant();
        if (h.Contains("google") || h.Contains("bing") || h.Contains("duckduckgo") || h.Contains("yahoo") || h.Contains("ecosia"))
            return "Buscas";
        if (h.Contains("facebook") || h.Contains("instagram") || h.Contains("twitter") || h.Contains("x.com")
            || h.Contains("youtube") || h.Contains("whatsapp") || h.Contains("linkedin") || h.Contains("tiktok"))
            return "Redes sociais";
        return "Outros sites";
    }
}
