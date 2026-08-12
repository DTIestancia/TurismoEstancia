using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Services.Conteudo.Interfaces;
using TurismoEstancia.Web.Models;

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

    public async Task<IActionResult> Criar(CancellationToken ct)
    {
        ViewData["Title"] = "Nova configuração";
        await PreencherChavesAsync(ViewData, ct);
        return View(new ConfiguracaoSiteDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(ConfiguracaoSiteDto dto, IFormFile? arquivo, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PreencherChavesAsync(ViewData, ct);
            return View(dto);
        }
        try
        {
            await _configuracoes.SalvarAsync(dto, arquivo, ct);
            TempData["PainelOk"] = "Configuração salva.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
            await PreencherChavesAsync(ViewData, ct);
            return View(dto);
        }
    }

    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        ViewData["Title"] = "Editar configuração";
        var dto = await _configuracoes.ObterPorIdAsync(id, ct);
        if (dto is null) return NotFound();
        await PreencherChavesAsync(ViewData, ct, dto.Chave);
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(ConfiguracaoSiteDto dto, IFormFile? arquivo, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PreencherChavesAsync(ViewData, ct, dto.Chave);
            return View(dto);
        }
        try
        {
            await _configuracoes.SalvarAsync(dto, arquivo, ct);
            TempData["PainelOk"] = "Configuração atualizada.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["PainelErro"] = ex.Message;
            await PreencherChavesAsync(ViewData, ct, dto.Chave);
            return View(dto);
        }
    }

    /// <summary>
    /// Chaves conhecidas do sistema (lidas pelos componentes do portal), para o
    /// Gerenciador escolher no select em vez de digitar. Ajuda a evitar chaves
    /// com typo que o portal simplesmente ignora. Chaves já cadastradas por outro
    /// registro aparecem desabilitadas no select (EmUso).
    /// </summary>
    private async Task PreencherChavesAsync(ViewDataDictionary viewData, CancellationToken ct, string? selecionada = null)
    {
        var emUso = (await _configuracoes.ListarAsync(ct))
            .Where(c => c.Chave != selecionada)
            .Select(c => c.Chave)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        viewData["ChavesDisponiveis"] = new List<ChaveDisponivelViewModel>
        {
            new() { Chave = "logo-principal", Nome = "Logotipo do portal (navbar e favicon)", EhArquivo = true, EmUso = emUso.Contains("logo-principal") },
            new() { Chave = "logo", Nome = "Logotipo do rodapé (e imagem do SEO)", EhArquivo = true, EmUso = emUso.Contains("logo") },
            new() { Chave = "favicon", Nome = "Favicon do site (PNG quadrado, ex.: 64x64)", EhArquivo = true, EmUso = emUso.Contains("favicon") },
            new() { Chave = "guia-pdf", Nome = "Guia do turista em PDF", EhArquivo = true, EmUso = emUso.Contains("guia-pdf") },
            new() { Chave = "video-institucional", Nome = "Vídeo institucional", EhArquivo = true, EmUso = emUso.Contains("video-institucional") },
            new() { Chave = "site-titulo", Nome = "Título do site (SEO e navegador)", EhArquivo = false, EmUso = emUso.Contains("site-titulo") },
            new() { Chave = "meta-descricao", Nome = "Meta description do site (SEO)", EhArquivo = false, EmUso = emUso.Contains("meta-descricao") },
            new() { Chave = "tema-cor-vermelho", Nome = "Cor do tema: vermelho", EhArquivo = false, EmUso = emUso.Contains("tema-cor-vermelho") },
            new() { Chave = "tema-cor-laranja", Nome = "Cor do tema: laranja", EhArquivo = false, EmUso = emUso.Contains("tema-cor-laranja") },
            new() { Chave = "tema-cor-amarelo", Nome = "Cor do tema: amarelo", EhArquivo = false, EmUso = emUso.Contains("tema-cor-amarelo") },
            new() { Chave = "tema-cor-verde", Nome = "Cor do tema: verde", EhArquivo = false, EmUso = emUso.Contains("tema-cor-verde") },
            new() { Chave = "tema-cor-azul", Nome = "Cor do tema: azul", EhArquivo = false, EmUso = emUso.Contains("tema-cor-azul") },
            new() { Chave = "tema-cor-rosa", Nome = "Cor do tema: rosa", EhArquivo = false, EmUso = emUso.Contains("tema-cor-rosa") }
        };
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
