using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Conteudo.Interfaces;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class ContatosController : PainelController
{
    private readonly IContatoService _contatos;

    public ContatosController(IServiceProvider services, IContatoService contatos)
        : base(services) => _contatos = contatos;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Contatos do rodapé";
        ViewData["AreaAtiva"] = "rodape";
        return View(await _contatos.ListarAsync(null, ct));
    }

    public async Task<IActionResult> Criar(CancellationToken ct)
    {
        ViewData["Title"] = "Novo contato";
        ViewData["AreaAtiva"] = "rodape";
        await PreencherRotulosAsync(ViewData, ct);
        return View(new ContatoDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(ContatoDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PreencherRotulosAsync(ViewData, ct);
            return View(dto);
        }
        await _contatos.SalvarAsync(dto, ct);
        TempData["PainelOk"] = "Contato salvo.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Editar contato";
        ViewData["AreaAtiva"] = "rodape";
        var dto = await _contatos.ObterPorIdAsync(id, ct);
        if (dto is null) return NotFound();
        await PreencherRotulosAsync(ViewData, ct, dto.Rotulo);
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(ContatoDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PreencherRotulosAsync(ViewData, ct, dto.Rotulo);
            return View(dto);
        }
        await _contatos.SalvarAsync(dto, ct);
        TempData["PainelOk"] = "Contato atualizado.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Rótulos comuns por tipo de contato (o rodapé exibe o rótulo ao lado do
    /// valor) + os já usados por outros contatos, desabilitados no select.
    /// </summary>
    private async Task PreencherRotulosAsync(ViewDataDictionary viewData, CancellationToken ct, string? selecionada = null)
    {
        var emUso = (await _contatos.ListarAsync(null, ct))
            .Where(c => !string.IsNullOrWhiteSpace(c.Rotulo) && !string.Equals(c.Rotulo, selecionada, StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Rotulo!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        viewData["ListaOpcoesKey"] = "RotulosDisponiveis";
        viewData["RotulosDisponiveis"] = new List<ChaveDisponivelViewModel>
        {
            new() { Chave = "Emergência", Nome = "Emergência", Grupo = "Telefone", EmUso = emUso.Contains("Emergência") },
            new() { Chave = "Turismo", Nome = "Turismo", Grupo = "Telefone", EmUso = emUso.Contains("Turismo") },
            new() { Chave = "Fale Conosco", Nome = "Fale Conosco", Grupo = "Telefone", EmUso = emUso.Contains("Fale Conosco") },
            new() { Chave = "Secretaria", Nome = "Secretaria", Grupo = "Telefone", EmUso = emUso.Contains("Secretaria") },
            new() { Chave = "Guarda Municipal", Nome = "Guarda Municipal", Grupo = "Telefone", EmUso = emUso.Contains("Guarda Municipal") },
            new() { Chave = "SAMU", Nome = "SAMU", Grupo = "Telefone", EmUso = emUso.Contains("SAMU") },
            new() { Chave = "SMTT", Nome = "SMTT", Grupo = "Telefone", EmUso = emUso.Contains("SMTT") },
            new() { Chave = "Prefeitura Municipal", Nome = "Prefeitura Municipal", Grupo = "Endereço", EmUso = emUso.Contains("Prefeitura Municipal") },
            new() { Chave = "Secretaria de Turismo", Nome = "Secretaria de Turismo", Grupo = "Endereço", EmUso = emUso.Contains("Secretaria de Turismo") },
            new() { Chave = "Endereço", Nome = "Endereço", Grupo = "Endereço", EmUso = emUso.Contains("Endereço") },
            new() { Chave = "Instagram", Nome = "Instagram", Grupo = "Rede social", EmUso = emUso.Contains("Instagram") },
            new() { Chave = "Facebook", Nome = "Facebook", Grupo = "Rede social", EmUso = emUso.Contains("Facebook") },
            new() { Chave = "YouTube", Nome = "YouTube", Grupo = "Rede social", EmUso = emUso.Contains("YouTube") },
            new() { Chave = "WhatsApp", Nome = "WhatsApp", Grupo = "Rede social", EmUso = emUso.Contains("WhatsApp") },
            new() { Chave = "TikTok", Nome = "TikTok", Grupo = "Rede social", EmUso = emUso.Contains("TikTok") },
            new() { Chave = "X (Twitter)", Nome = "X (Twitter)", Grupo = "Rede social", EmUso = emUso.Contains("X (Twitter)") }
        };
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
