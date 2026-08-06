using Microsoft.AspNetCore.Mvc;
using TurismoEstancia.Services.Avaliacao.Interfaces;
using TurismoEstancia.Web.Models;
using TurismoEstancia.Services.Comunicacao.Interfaces;
using TurismoEstancia.Services.CulturaGastronomia.Interfaces;
using TurismoEstancia.Services.Roteiro.Interfaces;
using TurismoEstancia.Services.Turismo.Interfaces;

namespace TurismoEstancia.Web.Areas.Gerenciador.Controllers;

public class DashboardController : PainelController
{
    private readonly IPontoTuristicoService _pontos;
    private readonly IEventoService _eventos;
    private readonly INoticiaService _noticias;
    private readonly IRoteiroService _roteiros;
    private readonly IInscricaoNewsletterService _newsletter;
    private readonly IAvaliacaoService _avaliacoes;
    private readonly IGrupoCulturalService _grupos;
    private readonly IPratoTuristicoService _pratos;
    private readonly ICategoriaPontoTuristicoService _categorias;

    public DashboardController(
        IServiceProvider services,
        IPontoTuristicoService pontos,
        IEventoService eventos,
        INoticiaService noticias,
        IRoteiroService roteiros,
        IInscricaoNewsletterService newsletter,
        IAvaliacaoService avaliacoes,
        IGrupoCulturalService grupos,
        IPratoTuristicoService pratos,
        ICategoriaPontoTuristicoService categorias)
        : base(services)
    {
        _pontos = pontos;
        _eventos = eventos;
        _noticias = noticias;
        _roteiros = roteiros;
        _newsletter = newsletter;
        _avaliacoes = avaliacoes;
        _grupos = grupos;
        _pratos = pratos;
        _categorias = categorias;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Dashboard";

        var itens = new List<PainelStatViewModel>
        {
            new() { Rotulo = "Pontos turísticos", Icone = "map-pin", Valor = (await _pontos.ListarAsync(apenasAtivos: false, ct)).Count },
            new() { Rotulo = "Categorias", Icone = "folder-tree", Valor = (await _categorias.ListarAsync(incluirInativos: true, ct)).Count },
            new() { Rotulo = "Eventos", Icone = "calendar", Valor = (await _eventos.ListarAsync(apenasProximos: false, ct)).Count },
            new() { Rotulo = "Notícias", Icone = "newspaper", Valor = (await _noticias.ListarAsync(apenasPublicadas: false, ct)).Count },
            new() { Rotulo = "Roteiros", Icone = "route", Valor = (await _roteiros.ListarAsync(ct)).Count },
            new() { Rotulo = "Grupos culturais", Icone = "music", Valor = (await _grupos.ListarAsync(ct)).Count },
            new() { Rotulo = "Pratos turísticos", Icone = "utensils", Valor = (await _pratos.ListarAsync(ct)).Count },
            new() { Rotulo = "Inscrições newsletter", Icone = "mail", Valor = (await _newsletter.ListarAsync(incluirInativos: true, ct)).Count },
            new() { Rotulo = "Avaliações", Icone = "star", Valor = (await _avaliacoes.ListarAsync(apenasAprovadas: false, ct)).Count }
        };

        return View(itens);
    }
}
