using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Conteudo.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class ContatosController : PainelController
{
    private readonly IContatoService _contatos;

    public ContatosController(IServiceProvider services, IContatoService contatos)
        : base(services) => _contatos = contatos;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Contatos do rodapé";
        return View(await _contatos.ListarAsync(null, ct));
    }

    public IActionResult Criar() => View(new ContatoDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(ContatoDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        await _contatos.SalvarAsync(dto, ct);
        TempData["PainelOk"] = "Contato salvo.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Editar contato";
        var dto = await _contatos.ObterPorIdAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(ContatoDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);
        await _contatos.SalvarAsync(dto, ct);
        TempData["PainelOk"] = "Contato atualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        await _contatos.ExcluirAsync(id, ct);
        TempData["PainelOk"] = "Contato excluído.";
        return RedirectToAction(nameof(Index));
    }
}
