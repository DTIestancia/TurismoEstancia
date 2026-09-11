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
        vm.SecaoHistoria = Valor(porChave, TemaViewModel.ChaveSecaoHistoria, "#C76527");
        vm.SecaoConheca = Valor(porChave, TemaViewModel.ChaveSecaoConheca, "#060D1A");
        vm.SecaoMaravilhas = Valor(porChave, TemaViewModel.ChaveSecaoMaravilhas, "#030E19");
        vm.SecaoAgenda = Valor(porChave, TemaViewModel.ChaveSecaoAgenda, "#320100");
        vm.SecaoRoteiros = Valor(porChave, TemaViewModel.ChaveSecaoRoteiros, "#001326");
        vm.SecaoNoticias = Valor(porChave, TemaViewModel.ChaveSecaoNoticias, "#FFFFFF");
        vm.SecaoMapa = Valor(porChave, TemaViewModel.ChaveSecaoMapa, "#030E19");
        vm.SecaoRodape = Valor(porChave, TemaViewModel.ChaveSecaoRodape, "#001326");
        vm.Personalizado = Cores.Concat(Secoes).Any(k => porChave.ContainsKey(k) && !string.IsNullOrWhiteSpace(porChave[k]));
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
        if (vm.SecaoHistoria is not null) await SalvarCorAsync(TemaViewModel.ChaveSecaoHistoria, "Fundo da seção Nossa Cidade", vm.SecaoHistoria, ct);
        if (vm.SecaoConheca is not null) await SalvarCorAsync(TemaViewModel.ChaveSecaoConheca, "Fundo da seção Conheça Estância", vm.SecaoConheca, ct);
        if (vm.SecaoMaravilhas is not null) await SalvarCorAsync(TemaViewModel.ChaveSecaoMaravilhas, "Fundo da seção 7 Maravilhas", vm.SecaoMaravilhas, ct);
        if (vm.SecaoAgenda is not null) await SalvarCorAsync(TemaViewModel.ChaveSecaoAgenda, "Fundo da seção Agenda", vm.SecaoAgenda, ct);
        if (vm.SecaoRoteiros is not null) await SalvarCorAsync(TemaViewModel.ChaveSecaoRoteiros, "Fundo da seção Planeje sua viagem", vm.SecaoRoteiros, ct);
        if (vm.SecaoNoticias is not null) await SalvarCorAsync(TemaViewModel.ChaveSecaoNoticias, "Fundo da seção Notícias e Blog", vm.SecaoNoticias, ct);
        if (vm.SecaoMapa is not null) await SalvarCorAsync(TemaViewModel.ChaveSecaoMapa, "Fundo da seção Mapa", vm.SecaoMapa, ct);
        if (vm.SecaoRodape is not null) await SalvarCorAsync(TemaViewModel.ChaveSecaoRodape, "Fundo do Rodapé", vm.SecaoRodape, ct);

        TempData["PainelOk"] = "Tema atualizado. As mudanças já valem no portal, no painel e no login.";
        return RedirecionarParaIndex();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restaurar(CancellationToken ct)
    {
        var todas = await _configuracoes.ListarAsync(ct);
        foreach (var item in todas.Where(c => c.Chave.StartsWith("tema-cor-", StringComparison.Ordinal)
            || c.Chave.StartsWith("tema-secao-", StringComparison.Ordinal)))
            await _configuracoes.ExcluirAsync(item.Id, ct);

        TempData["PainelOk"] = "Paleta oficial restaurada.";
        return RedirecionarParaIndex();
    }

    private static readonly string[] Cores =
    [
        TemaViewModel.ChaveVermelho, TemaViewModel.ChaveLaranja, TemaViewModel.ChaveAmarelo,
        TemaViewModel.ChaveVerde, TemaViewModel.ChaveAzul, TemaViewModel.ChaveRosa
    ];

    private static readonly string[] Secoes =
    [
        TemaViewModel.ChaveSecaoHistoria, TemaViewModel.ChaveSecaoConheca,
        TemaViewModel.ChaveSecaoMaravilhas, TemaViewModel.ChaveSecaoAgenda,
        TemaViewModel.ChaveSecaoRoteiros, TemaViewModel.ChaveSecaoNoticias,
        TemaViewModel.ChaveSecaoMapa, TemaViewModel.ChaveSecaoRodape
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
