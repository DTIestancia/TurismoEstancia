using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Web.Infrastructure;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Conteudo.Interfaces;
using TurismoEstancia.Services.Turismo.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class PontosTuristicosController : PainelController
{
    private readonly IPontoTuristicoService _pontos;
    private readonly ICategoriaPontoTuristicoService _categorias;
    private readonly IConteudoSiteService _conteudos;

    public PontosTuristicosController(
        IServiceProvider services,
        IPontoTuristicoService pontos,
        ICategoriaPontoTuristicoService categorias,
        IConteudoSiteService conteudos)
        : base(services)
    {
        _pontos = pontos;
        _categorias = categorias;
        _conteudos = conteudos;
    }

    public async Task<IActionResult> Index(string? contexto, CancellationToken ct, int pagina = 1)
    {
        ViewData["Title"] = "Pontos turísticos";
        // O contexto separa os dois usos no portal: "maravilhas" (7 Maravilhas)
        // e "mapa" (todos os pontos com ExibirNoMapa — restaurantes, hotéis...).
        ViewData["AreaAtiva"] = contexto == "mapa" ? "mapa" : "maravilhas";
        ViewData["ContextoPontos"] = contexto;

        var todos = await _pontos.ListarAsync(apenasAtivos: false, ct);
        var lista = contexto switch
        {
            "maravilhas" => todos.Where(p => p.CategoriaApresentarEmMaravilhas).ToList(),
            "mapa" => todos.Where(p => p.ExibirNoMapa).ToList(),
            _ => todos
        };

        var totalPaginas = Math.Max(1, (int)Math.Ceiling(lista.Count / (double)PaginaService.TamanhoPainel));
        var paginaAtual = Math.Clamp(pagina, 1, totalPaginas);
        ViewData["PaginaAtual"] = paginaAtual;
        ViewData["PaginasTotal"] = totalPaginas;

        return View(lista
            .Skip((paginaAtual - 1) * PaginaService.TamanhoPainel)
            .Take(PaginaService.TamanhoPainel)
            .ToList());
    }

    public async Task<IActionResult> Criar(CancellationToken ct)
    {
        ViewData["Title"] = "Novo ponto turístico";
        ViewData["AreaAtiva"] = "maravilhas";
        ViewBag.Categorias = await _categorias.ListarAsync(incluirInativos: true, ct);
        ViewBag.MapaImagemId = await ObterMapaImagemIdAsync(ct);
        ViewBag.MapaImagemIdMobile = await ObterMapaImagemIdMobileAsync(ct);
        return View(new PontoTuristicoDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(
        PontoTuristicoDto dto,
        IFormFile? capa,
        IFormFile? pictograma,
        IEnumerable<IFormFile> galeria,
        IFormFile? icone,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = await _categorias.ListarAsync(incluirInativos: true, ct);
            return View(dto);
        }

        try
        {
            await _pontos.SalvarAsync(dto, capa, pictograma, galeria, icone, ct);
            TempData["PainelOk"] = "Ponto turístico salvo.";
            return RedirecionarParaIndex();
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
            ViewBag.Categorias = await _categorias.ListarAsync(incluirInativos: true, ct);
            return View(dto);
        }
    }

    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Editar ponto turístico";
        ViewData["AreaAtiva"] = "maravilhas";
        ViewBag.Categorias = await _categorias.ListarAsync(incluirInativos: true, ct);
        ViewBag.MapaImagemId = await ObterMapaImagemIdAsync(ct);
        ViewBag.MapaImagemIdMobile = await ObterMapaImagemIdMobileAsync(ct);
        var dto = await _pontos.ObterPorIdAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }

    private async Task<long?> ObterMapaImagemIdAsync(CancellationToken ct)
    {
        var d = await _conteudos.ObterDicionarioAsync(ct);
        return d.TryGetValue("mapa-imagem", out var v) && long.TryParse(v, out var id) && id > 0 ? id : null;
    }
    private async Task<long?> ObterMapaImagemIdMobileAsync(CancellationToken ct)
    {
        var d = await _conteudos.ObterDicionarioAsync(ct);
        return d.TryGetValue("mapa-imagem-mobile", out var v) && long.TryParse(v, out var id) && id > 0 ? id : null;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(
        PontoTuristicoDto dto,
        IFormFile? capa,
        IFormFile? pictograma,
        IEnumerable<IFormFile> galeria,
        IFormFile? icone,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = await _categorias.ListarAsync(incluirInativos: true, ct);
            return View(dto);
        }

        try
        {
            await _pontos.SalvarAsync(dto, capa, pictograma, galeria, icone, ct);
            TempData["PainelOk"] = "Ponto turístico atualizado.";
            return RedirecionarParaIndex();
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
            ViewBag.Categorias = await _categorias.ListarAsync(incluirInativos: true, ct);
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ocultar(int id, string? contexto, CancellationToken ct)
    {
        try
        {
            await _pontos.OcultarAsync(id, ct);
            TempData["PainelOk"] = "Ponto turístico ocultado.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { contexto });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reativar(int id, string? contexto, CancellationToken ct)
    {
        try
        {
            await _pontos.ReativarAsync(id, ct);
            TempData["PainelOk"] = "Ponto turístico reativado.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { contexto });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, string? contexto, CancellationToken ct)
    {
        try
        {
            await _pontos.ExcluirAsync(id, ct);
            TempData["PainelOk"] = "Ponto turístico excluído definitivamente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { contexto });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AtualizarPosicao(int id, int leftPercent, int topPercent, CancellationToken ct)
    {
        try
        {
            await _pontos.AtualizarPosicaoAsync(id, leftPercent, topPercent, ct);
            return Json(new { ok = true, leftPercent = Math.Clamp(leftPercent, 0, 100), topPercent = Math.Clamp(topPercent, 0, 100) });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { ok = false, erro = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AtualizarPosicaoMobile(int id, int leftPercent, int topPercent, int leftPercentMobile, int topPercentMobile, CancellationToken ct)
    {
        // Se vier com leftPercentMobile (do editor geral mobile), usa-o; senão usa leftPercent
        var x = leftPercentMobile != 0 || topPercentMobile != 0 ? leftPercentMobile : leftPercent;
        var y = topPercentMobile != 0 || leftPercentMobile != 0 ? topPercentMobile : topPercent;
        if (x == 0 && leftPercent != 0) x = leftPercent;
        if (y == 0 && topPercent != 0) y = topPercent;
        try
        {
            await _pontos.AtualizarPosicaoMobileAsync(id, x, y, ct);
            return Json(new { ok = true, leftPercentMobile = Math.Clamp(x, 0, 100), topPercentMobile = Math.Clamp(y, 0, 100) });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { ok = false, erro = ex.Message });
        }
    }
}
