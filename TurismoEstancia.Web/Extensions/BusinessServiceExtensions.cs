using TurismoEstancia.Services.Avaliacao.Interfaces;
using TurismoEstancia.Services.Avaliacao.Services;
using TurismoEstancia.Services.Comunicacao.Interfaces;
using TurismoEstancia.Services.Comunicacao.Services;
using TurismoEstancia.Services.Conteudo.Interfaces;
using TurismoEstancia.Services.Conteudo.Services;
using TurismoEstancia.Services.CulturaGastronomia.Interfaces;
using TurismoEstancia.Services.CulturaGastronomia.Services;
using TurismoEstancia.Services.Infra.Interfaces;
using TurismoEstancia.Services.Infra.Services;
using TurismoEstancia.Services.Roteiro.Interfaces;
using TurismoEstancia.Services.Roteiro.Services;
using TurismoEstancia.Services.Turismo.Interfaces;
using TurismoEstancia.Services.Turismo.Services;

namespace TurismoEstancia.Web.Extensions;

/// <summary>
/// Registra todos os serviços de negócio, agrupados por módulo.
/// </summary>
public static class BusinessServiceExtensions
{
    public static void AddBusinessServices(this WebApplicationBuilder builder)
    {
        // Infra
        builder.Services.AddScoped<IArquivoService, ArquivoService>();

        // Módulo Turismo
        builder.Services.AddScoped<ICategoriaPontoTuristicoService, CategoriaPontoTuristicoService>();
        builder.Services.AddScoped<IPontoTuristicoService, PontoTuristicoService>();
        builder.Services.AddScoped<IEventoService, EventoService>();
        builder.Services.AddScoped<ISlideService, SlideService>();
        builder.Services.AddScoped<IEstatisticaService, EstatisticaService>();

        // Módulo CulturaGastronomia
        builder.Services.AddScoped<IGrupoCulturalService, GrupoCulturalService>();
        builder.Services.AddScoped<IPratoTuristicoService, PratoTuristicoService>();
        builder.Services.AddScoped<ITagCulturalService, TagCulturalService>();

        // Módulo Conteudo
        builder.Services.AddScoped<IConteudoSiteService, ConteudoSiteService>();
        builder.Services.AddScoped<IConfiguracaoSiteService, ConfiguracaoSiteService>();
        builder.Services.AddScoped<IContatoService, ContatoService>();

        // Módulo Comunicacao
        builder.Services.AddScoped<IInscricaoNewsletterService, InscricaoNewsletterService>();
        builder.Services.AddScoped<INoticiaService, NoticiaService>();

        // Módulo Roteiro
        builder.Services.AddScoped<IRoteiroService, RoteiroService>();

        // Módulo Avaliacao
        builder.Services.AddScoped<IAvaliacaoService, AvaliacaoService>();

        // Novos módulos serão adicionados aqui.
    }
}
