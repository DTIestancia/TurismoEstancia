using Microsoft.AspNetCore.Mvc;
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

    public async Task<IActionResult> Index(string? contexto, CancellationToken ct)
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
        return View(lista);
    }

    public async Task<IActionResult> Criar(CancellationToken ct)
    {
        ViewData["Title"] = "Novo ponto turístico";
        ViewData["AreaAtiva"] = "maravilhas";
        ViewBag.Categorias = await _categorias.ListarAsync(incluirInativos: true, ct);
        ViewBag.MapaImagemId = await ObterMapaImagemIdAsync(ct);
        return View(new PontoTuristicoDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(
        PontoTuristicoDto dto,
        IFormFile? capa,
        IFormFile? pictograma,
        IEnumerable<IFormFile> galeria,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = await _categorias.ListarAsync(incluirInativos: true, ct);
            return View(dto);
        }

        try
        {
            await _pontos.SalvarAsync(dto, capa, pictograma, galeria, ct);
            TempData["PainelOk"] = "Ponto turístico salvo.";
            return RedirectToAction(nameof(Index));
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
        var dto = await _pontos.ObterPorIdAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }

    private async Task<long?> ObterMapaImagemIdAsync(CancellationToken ct)
    {
        var d = await _conteudos.ObterDicionarioAsync(ct);
        return d.TryGetValue("mapa-imagem", out var v) && long.TryParse(v, out var id) && id > 0 ? id : null;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(
        PontoTuristicoDto dto,
        IFormFile? capa,
        IFormFile? pictograma,
        IEnumerable<IFormFile> galeria,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = await _categorias.ListarAsync(incluirInativos: true, ct);
            return View(dto);
        }

        try
        {
            await _pontos.SalvarAsync(dto, capa, pictograma, galeria, ct);
            TempData["PainelOk"] = "Ponto turístico atualizado.";
            return RedirectToAction(nameof(Index));
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
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        await _pontos.ExcluirAsync(id, ct);
        TempData["PainelOk"] = "Ponto turístico desativado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reativar(int id, CancellationToken ct)
    {
        await _pontos.ReativarAsync(id, ct);
        TempData["PainelOk"] = "Ponto turístico reativado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AtualizarPosicao(int id, int leftPercent, int topPercent, CancellationToken ct)
    {
        await _pontos.AtualizarPosicaoAsync(id, leftPercent, topPercent, ct);
        return Json(new { ok = true, leftPercent = Math.Clamp(leftPercent, 0, 100), topPercent = Math.Clamp(topPercent, 0, 100) });
    }
}
