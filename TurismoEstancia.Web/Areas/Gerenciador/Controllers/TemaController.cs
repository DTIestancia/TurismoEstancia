using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Domain.DTOs;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Services.Conteudo.Interfaces;
using TurismoEstancia.Web.Models;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

/// <summary>
/// Tema do portal: permite ao Gerenciador personalizar as 6 cores da paleta
/// oficial sem recompilar o SCSS. As cores são persistidas como configurações
/// (chave <c>tema-cor-*</c>, tipo Texto) e aplicadas em runtime pelo
/// <c>ThemeSiteViewComponent</c> (portal, painel e login).
/// </summary>
public class TemaController : PainelController
{
    private readonly IConfiguracaoSiteService _configuracoes;

    public TemaController(IServiceProvider services, IConfiguracaoSiteService configuracoes)
        : base(services) => _configuracoes = configuracoes;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Tema e cores";
        var vm = new TemaViewModel();
        var todas = await _configuracoes.ListarAsync(ct);
        var porChave = todas.ToDictionary(c => c.Chave, c => c.ValorTexto);

        vm.Vermelho = Valor(porChave, TemaViewModel.ChaveVermelho, "#ED2027");
        vm.Laranja = Valor(porChave, TemaViewModel.ChaveLaranja, "#F97E31");
        vm.Amarelo = Valor(porChave, TemaViewModel.ChaveAmarelo, "#FCBB0F");
        vm.Verde = Valor(porChave, TemaViewModel.ChaveVerde, "#658746");
        vm.Azul = Valor(porChave, TemaViewModel.ChaveAzul, "#0095F6");
        vm.Rosa = Valor(porChave, TemaViewModel.ChaveRosa, "#E9568A");
        vm.Personalizado = Cores.Any(k => porChave.ContainsKey(k) && !string.IsNullOrWhiteSpace(porChave[k]));
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Salvar(TemaViewModel vm, CancellationToken ct)
    {
        if (vm.Vermelho is not null) await SalvarCorAsync(TemaViewModel.ChaveVermelho, "Cor vermelha (tema)", vm.Vermelho, ct);
        if (vm.Laranja is not null) await SalvarCorAsync(TemaViewModel.ChaveLaranja, "Cor laranja (tema)", vm.Laranja, ct);
        if (vm.Amarelo is not null) await SalvarCorAsync(TemaViewModel.ChaveAmarelo, "Cor amarela (tema)", vm.Amarelo, ct);
        if (vm.Verde is not null) await SalvarCorAsync(TemaViewModel.ChaveVerde, "Cor verde (tema)", vm.Verde, ct);
        if (vm.Azul is not null) await SalvarCorAsync(TemaViewModel.ChaveAzul, "Cor azul (tema)", vm.Azul, ct);
        if (vm.Rosa is not null) await SalvarCorAsync(TemaViewModel.ChaveRosa, "Cor rosa (tema)", vm.Rosa, ct);

        TempData["PainelOk"] = "Tema atualizado. As mudanças já valem no portal, no painel e no login.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restaurar(CancellationToken ct)
    {
        var todas = await _configuracoes.ListarAsync(ct);
        foreach (var item in todas.Where(c => c.Chave.StartsWith("tema-cor-", StringComparison.Ordinal)))
            await _configuracoes.ExcluirAsync(item.Id, ct);

        TempData["PainelOk"] = "Paleta oficial restaurada.";
        return RedirectToAction(nameof(Index));
    }

    private static readonly string[] Cores =
    [
        TemaViewModel.ChaveVermelho, TemaViewModel.ChaveLaranja, TemaViewModel.ChaveAmarelo,
        TemaViewModel.ChaveVerde, TemaViewModel.ChaveAzul, TemaViewModel.ChaveRosa
    ];

    private static string Valor(IReadOnlyDictionary<string, string?> porChave, string chave, string padrao)
    {
        var valor = porChave.TryGetValue(chave, out var v) ? v : null;
        return !string.IsNullOrWhiteSpace(valor) ? valor : padrao;
    }

    private async Task SalvarCorAsync(string chave, string nome, string hex, CancellationToken ct)
    {
        var existente = await _configuracoes.ObterPorChaveAsync(chave, ct);
        await _configuracoes.SalvarAsync(new ConfiguracaoSiteDto
        {
            Id = existente?.Id ?? 0,
            Chave = chave,
            Nome = existente?.Nome ?? nome,
            Tipo = TipoConfiguracao.Texto,
            ValorTexto = hex
        }, null, ct);
    }
}
