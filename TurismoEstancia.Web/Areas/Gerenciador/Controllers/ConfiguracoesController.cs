using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Conteudo.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class ConfiguracoesController : PainelController
{
    private readonly IConfiguracaoSiteService _configuracoes;

    public ConfiguracoesController(IServiceProvider services, IConfiguracaoSiteService configuracoes)
        : base(services) => _configuracoes = configuracoes;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Configurações";
        return View(await _configuracoes.ListarAsync(ct));
    }

    public IActionResult Criar() => View(new ConfiguracaoSiteDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(ConfiguracaoSiteDto dto, IFormFile? arquivo, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _configuracoes.SalvarAsync(dto, arquivo, ct);
            TempData["PainelOk"] = "Configuração salva.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
            return View(dto);
        }
    }

    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Editar configuração";
        var dto = await _configuracoes.ObterPorIdAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(ConfiguracaoSiteDto dto, IFormFile? arquivo, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        try
        {
            await _configuracoes.SalvarAsync(dto, arquivo, ct);
            TempData["PainelOk"] = "Configuração atualizada.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        await _configuracoes.ExcluirAsync(id, ct);
        TempData["PainelOk"] = "Configuração excluída.";
        return RedirectToAction(nameof(Index));
    }
}
