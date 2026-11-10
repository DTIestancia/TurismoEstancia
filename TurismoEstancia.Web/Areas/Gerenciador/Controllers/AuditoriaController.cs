using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurismoEstancia.Authorization.Services;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

[Area("Gerenciador")]
[Authorize(Policy = Perfis.DTI)]
public sealed class AuditoriaController : Controller
{
    private const int TamanhoPagina = 50;
    private readonly AppDbContext _db;

    public AuditoriaController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index(
        string? tipo,
        string? area,
        string? usuario,
        DateTime? inicio,
        DateTime? fim,
        int pagina = 1,
        CancellationToken cancellationToken = default)
    {
        pagina = Math.Max(1, pagina);
        var consulta = _db.Auditorias.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(tipo)) consulta = consulta.Where(a => a.Tipo == tipo);
        if (!string.IsNullOrWhiteSpace(area)) consulta = consulta.Where(a => a.Area == area);
        if (!string.IsNullOrWhiteSpace(usuario))
            consulta = consulta.Where(a => a.UsuarioNome != null && a.UsuarioNome.Contains(usuario));
        if (inicio.HasValue) consulta = consulta.Where(a => a.DataHoraUtc >= inicio.Value.ToUniversalTime());
        if (fim.HasValue) consulta = consulta.Where(a => a.DataHoraUtc < fim.Value.Date.AddDays(1).ToUniversalTime());

        var total = await consulta.CountAsync(cancellationToken);
        var acessos = await consulta.CountAsync(a => a.Tipo == "EntradaPainel" || a.Tipo == "AcessoTela", cancellationToken);
        var alteracoes = await consulta.CountAsync(a => a.Tipo == "Criacao" || a.Tipo == "Alteracao" || a.Tipo == "Exclusao", cancellationToken);
        var falhas = await consulta.CountAsync(a => !a.Sucesso, cancellationToken);
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamanhoPagina));
        pagina = Math.Min(pagina, totalPaginas);
        var registros = await consulta
            .OrderByDescending(a => a.DataHoraUtc)
            .Skip((pagina - 1) * TamanhoPagina)
            .Take(TamanhoPagina)
            .ToListAsync(cancellationToken);

        return View(new AuditoriaListaViewModel
        {
            Registros = registros,
            Tipo = tipo,
            Area = area,
            Usuario = usuario,
            Inicio = inicio,
            Fim = fim,
            Pagina = pagina,
            TotalPaginas = totalPaginas,
            TotalRegistros = total,
            TotalAcessos = acessos,
            TotalAlteracoes = alteracoes,
            TotalFalhas = falhas
        });
    }
}
