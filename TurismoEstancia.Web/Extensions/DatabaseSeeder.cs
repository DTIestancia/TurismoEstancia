using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TurismoEstancia.Domain.Data;
using TurismoEstancia.Domain.Models;
using TurismoEstancia.Identity.Data;
using TurismoEstancia.Identity.Models;

namespace TurismoEstancia.Web.Extensions;

/// <summary>
/// Popula os bancos com o conteúdo do protótipo (Prototipo/img + index.html).
/// Executado com `dotnet run -- --seed`. Idempotente: nada é sobrescrito
/// quando os registros já existem.
/// </summary>
public static class DatabaseSeeder
{
    private static readonly string ImgPath = Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "Prototipo", "img");

    /// <summary>Raiz do seed: aplica migrações e popula os dados.</summary>
    public static async Task SeedAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var identity = scope.ServiceProvider.GetRequiredService<IdentityContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();

        // Mídias grandes (vídeo 15MB) e lote extenso: comando com timeout maior.
        db.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
        identity.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

        await db.Database.MigrateAsync();
        await identity.Database.MigrateAsync();

        // Admin sempre garantido (mesmo se o banco de negócio já estiver populado).
        await GarantirAdminAsync(userManager);

        if (await db.CategoriasPontosTuristicos.AnyAsync() && await db.Arquivos.AnyAsync())
        {
            Console.WriteLine("[seed] Banco já populado — nada a fazer.");
            return;
        }

        // ---------- Mídias (arquivos do protótipo) ----------
        var capas = await SalvarCapasAsync(db);
        var pictos = await SalvarPictogramasAsync(db);
        var slide1 = await SalvarArquivoAsync(db, "CAPITAL BRASILEIRA DO BARCO DE FOGO.png", "image/png");
        var slide2 = await SalvarArquivoAsync(db, "praia do saco.jpeg", "image/jpeg");
        var slide3 = await SalvarArquivoAsync(db, "barcos de fogo.jpeg", "image/jpeg");
        var logo = await SalvarArquivoAsync(db, "ASSINATURA-CAPITAL-MARAVILHAS-AZUL.png", "image/png");
        var video = await SalvarArquivoAsync(db, "video_institucional.mp4", "video/mp4");
        // TODO: trocar por um PDF real do guia do turista (placeholder usa uma imagem).
        var guia = await SalvarArquivoAsync(db, "CAPITAL BRASILEIRA DO BARCO DE FOGO.png", "image/png");

        // ---------- Categorias ----------
        var catHeritage = AddCategoria(db, "heritage", "Patrimônio & História", "A história viva de Estância", "#F76400", "landmark", 1);
        var catNature = AddCategoria(db, "nature", "Natureza & Praias", "Belezas naturais de tirar o fôlego", "#009662", "tree-pine", 2);
        var catHotel = AddCategoria(db, "hotel", "Hospedagem", "Onde descansar", "#009CCF", "bed", 3, maravilhas: false);
        var catFood = AddCategoria(db, "food", "Gastronomia", "Sabores do litoral sergipano", "#E63946", "utensils", 4, maravilhas: false);
        var catService = AddCategoria(db, "service", "Serviços", "Informações e apoio ao turista", "#6C63FF", "info", 5, maravilhas: false);

        // Persiste as categorias para obter os Ids reais (FKs dos pontos).
        await db.SaveChangesAsync();

        // ---------- 7 Maravilhas ----------
        AddPonto(db, catHeritage, "Lira Carlos Gomes",
            "Filarmônica centenária fundada em 1879, símbolo da música local.",
            "Mais de um século de história musical, a Lira Carlos Gomes é um dos mais importantes símbolos culturais de Estância, embalando gerações com suas apresentações e mantendo viva a tradição das bandas filarmônicas sergipanas.",
            "🎺 Música", "sparkles", 47, 45, 1, "Praça Barão do Rio Branco, s/n - Centro, Estância - SE",
            "Localizada no centro histórico, a poucos passos da Catedral. Estacionamento gratuito disponível nas ruas adjacentes ao redor da praça.",
            capas["lira"], pictos["lira"]);

        AddPonto(db, catHeritage, "Catedral Nossa Senhora de Guadalupe",
            "Igreja do século XVIII com traços históricos no centro da cidade.",
            "A Catedral Diocesana Nossa Senhora de Guadalupe é um marco da arquitetura religiosa colonial, com sua fachada imponente e interior ricamente decorado. Construída no século XVIII, é ponto central das procissões e festejos religiosos que movimentam a cidade durante todo o ano.",
            "✝ Fé", "church", 51, 48, 2, "Praça da Matriz, s/n - Centro, Estância - SE",
            "No coração da cidade, de fácil acesso a partir de qualquer ponto do centro histórico.",
            capas["igreja"], pictos["igreja"]);

        AddPonto(db, catHeritage, "Complexo da Fábrica Santa Cruz",
            "Conhecida como Fábrica Velha, marco da industrialização de 1891.",
            "A Fábrica Santa Cruz, carinhosamente chamada de Fábrica Velha, foi inaugurada em 1891 e representa o marco da industrialização têxtil em Sergipe. Suas imponentes ruínas de tijolos à vista contam a história do desenvolvimento econômico de Estância.",
            "🏭 História", "factory", 46, 38, 3, "Av. Presidente Vargas, s/n - Centro, Estância - SE",
            "Próximo ao centro, siga pela Av. Presidente Vargas sentido norte. O complexo fica à direita, com estacionamento no local.",
            capas["fabrica"], pictos["fabrica"]);

        AddPonto(db, catHeritage, "Barco de Fogo",
            "Artefato pirotécnico tradicional que tornou a cidade capital nacional.",
            "Estância é reconhecida como a Capital Nacional do Barco de Fogo, uma tradição centenária que ilumina o céu da cidade com espetáculos únicos de fogos de artifício artesanais.",
            "🔥 Tradição", "sparkles", 53, 44, 4, "Largo do Barco de Fogo - Centro, Estância - SE",
            "No período junino, o evento principal ocorre na orla da cidade. Durante o resto do ano, visite o Memorial do Barco de Fogo no centro.",
            capas["barco"], pictos["barco"]);

        AddPonto(db, catNature, "Lagoa dos Tambaquis",
            "Ponto turístico com águas calmas e contato direto com peixes.",
            "A Lagoa dos Tambaquis é um dos points naturais mais queridos de Estância. Com águas calmas e cristalinas, o local oferece uma experiência única de interação com os tambaquis, peixes mansos que se aproximam dos visitantes.",
            "💧 Natureza", "fish", 38, 70, 5, "Rodovia Estância-Praia do Saco, Zona Rural, Estância - SE",
            "Siga pela rodovia em direção à Praia do Saco por aproximadamente 8 km. A lagoa fica à esquerda, com placa indicativa. Acesso de carro recomendado.",
            capas["lagoa"], pictos["lagoa"]);

        AddPonto(db, catNature, "Praia do Saco",
            "Reconhecida pela beleza singular de suas dunas e faixa de areia.",
            "Com mar calmo, coqueirais e uma extensa faixa de areia, a Praia do Saco é o principal destino de banho de mar em Estância. Suas dunas e o pôr do sol deslumbrante fazem deste paraíso um lugar inesquecível.",
            "🌊 Praia", "umbrella", 48, 86, 6, "Praia do Saco, Zona Rural, Estância - SE",
            "Siga pela rodovia SE-100 sentido sul por aproximadamente 15 km. A praia fica à direita, bem sinalizada.",
            capas["saco"], pictos["saco"]);

        AddPonto(db, catNature, "Complexo Turístico do Porto D'Areia",
            "Espaço de lazer e esporte às margens do Rio Piauitinga.",
            "O Complexo Turístico do Porto D'Areia oferece infraestrutura completa para práticas esportivas, caminhadas e momentos de contemplação às margens do Rio Piauitinga.",
            "🌳 Lazer", "sailboat", 68, 55, 7, "Av. Beira Rio, Porto D'Areia, Estância - SE",
            "Siga pela Av. Beira Rio sentido leste. O complexo fica às margens do rio, com amplo estacionamento gratuito e acesso para pedestres.",
            capas["cristo"], pictos["cristo"]);

        // ---------- POIs do mapa (hotéis, restaurantes, serviços) ----------
        AddPonto(db, catHotel, "Hotel Centro Histórico",
            "Hospedagem aconchegante com vista para o centro histórico de Estância.",
            "Localizado a poucos passos da Catedral e da Praça Barão do Rio Branco, o Hotel Centro Histórico oferece quartos confortáveis com ar-condicionado, café da manhã incluso e estacionamento gratuito.",
            "🏨 Hotel", "bed", 50, 35, 8, "Rua da Matriz, 45 - Centro, Estância - SE",
            "No centro histórico, a 2 quadras da Catedral. Estacionamento próprio disponível.",
            null, null, exibirMapa: true);

        AddPonto(db, catHotel, "Pousada Praia do Saco",
            "Pousada charmosa à beira-mar com vista privilegiada.",
            "A Pousada Praia do Saco é o refúgio perfeito para quem busca sossego e contato com a natureza. Com quartos aconchegantes, rede de descanso na varanda e café da manhã regional.",
            "🏨 Pousada", "bed", 45, 85, 9, "Av. Beira Mar, s/n - Praia do Saco, Estância - SE",
            "Na orla da Praia do Saco, de frente para o mar. Fácil acesso pela rodovia SE-100.",
            null, null, exibirMapa: true);

        AddPonto(db, catFood, "Restaurante do Porto",
            "Culinária regional com peixes e frutos do mar frescos do dia.",
            "O Restaurante do Porto é referência em gastronomia típica sergipana. Com ambiente rústico e aconchegante, serve pratos como moqueca de camarão, peixe frito na telha e a tradicional ginga com tapioca.",
            "🍽️ Gastronomia", "utensils-crossed", 55, 52, 10, "Av. Beira Rio, 100 - Porto D'Areia, Estância - SE",
            "Às margens do Rio Piauitinga, ao lado do Complexo Turístico do Porto D'Areia.",
            null, null, exibirMapa: true);

        AddPonto(db, catFood, "Sorveteria & Lanchonete Central",
            "Sorvetes artesanais e lanches rápidos no coração da cidade.",
            "A Sorveteria Central é point obrigatório para quem visita Estância. Com mais de 30 sabores de sorvete artesanal, incluindo frutas típicas da região como mangaba e cajá.",
            "🍦 Sorveteria", "ice-cream", 48, 40, 11, "Praça Barão do Rio Branco, s/n - Centro, Estância - SE",
            "Na praça central de Estância, em frente à Prefeitura.",
            null, null, exibirMapa: true);

        AddPonto(db, catService, "Posto de Informações Turísticas",
            "Informações sobre passeios, hospedagem e eventos da cidade.",
            "O Posto de Informações Turísticas de Estância oferece mapas, guias e informações atualizadas sobre os pontos turísticos, hospedagem, restaurantes e eventos da cidade.",
            "ℹ️ Turismo", "info", 50, 43, 12, "Praça da Matriz, s/n - Centro, Estância - SE",
            "Ao lado da Catedral, no centro da cidade. Funciona de segunda a sábado, das 8h às 18h.",
            null, null, exibirMapa: true);

        // ---------- Slides do hero ----------
        db.Slides.AddRange(
            new Slide { ImagemArquivoId = slide1, Titulo = "Capital Brasileira do Barco de Fogo", Ordem = 1 },
            new Slide { ImagemArquivoId = slide2, Titulo = "Praia do Saco", Ordem = 2 },
            new Slide { ImagemArquivoId = slide3, Titulo = "Barcos de Fogo", Ordem = 3 });

        // ---------- Estatísticas ----------
        db.Estatisticas.AddRange(
            new Estatistica { Valor = "192", Legenda = "anos de história", Ordem = 1 },
            new Estatistica { Valor = "68k", Legenda = "habitantes", Ordem = 2 },
            new Estatistica { Valor = "7", Legenda = "maravilhas", Ordem = 3 });

        // ---------- Cultura & Gastronomia ----------
        db.GruposCulturais.AddRange(
            new GrupoCultural { Nome = "Reisado", Ordem = 1 },
            new GrupoCultural { Nome = "Cacumbi", Ordem = 2 },
            new GrupoCultural { Nome = "Batucada", Ordem = 3 },
            new GrupoCultural { Nome = "Quadrilhas Juninas", Ordem = 4 });

        db.PratosTuristicos.AddRange(
            new PratoTuristico { Nome = "Ginga com Tapioca", Ordem = 1 },
            new PratoTuristico { Nome = "Bolo de Macaxeira", Ordem = 2 },
            new PratoTuristico { Nome = "Moqueca de Camarão", Ordem = 3 },
            new PratoTuristico { Nome = "Peixe Frito na Telha", Ordem = 4 });

        db.TagsCulturais.AddRange(
            new TagCultural { Nome = "Barco de Fogo", Ordem = 1 },
            new TagCultural { Nome = "Filarmônicas", Ordem = 2 },
            new TagCultural { Nome = "São João", Ordem = 3 },
            new TagCultural { Nome = "Procissões", Ordem = 4 },
            new TagCultural { Nome = "Quadrilhas", Ordem = 5 },
            new TagCultural { Nome = "Batucadas", Ordem = 6 });

        // ---------- Textos do portal (ConteudosSite) ----------
        db.ConteudosSite.AddRange(
            new ConteudoSite
            {
                Chave = "hero-titulo", Nome = "Título do hero",
                Texto = "Explore as Cores, a História e a Tradição de Estância"
            },
            new ConteudoSite
            {
                Chave = "hero-subtitulo", Nome = "Subtítulo do hero",
                Texto = "Entre dunas douradas e céus que viram arte, Estância te chama pra viver uma experiência que acende todos os sentidos. O Nordeste está aqui, mais vibrante do que nunca."
            },
            new ConteudoSite
            {
                Chave = "historia-texto", Nome = "Texto da história",
                Texto = "Sente o vento salgado da Praia do Saco, escuta o estalar dos Barcos de Fogo no céu. Aqui, cada esquina é uma descoberta, cada sorriso é um convite. Vem viver Estância."
            },
            new ConteudoSite
            {
                Chave = "maravilhas-descricao", Nome = "Descrição das Maravilhas",
                Texto = "De filarmônicas centenárias a praias de tirar o fôlego — cada cantinho de Estância tem uma história esperando por você."
            },
            new ConteudoSite
            {
                Chave = "mapa-descricao", Nome = "Descrição do mapa",
                Texto = "Clique nos marcadores e descubra onde cada maravilha está esperando por você, com endereço e dicas de como chegar. Bora explorar?"
            },
            new ConteudoSite
            {
                Chave = "newsletter-titulo", Nome = "Título da newsletter",
                Texto = "Receba a programação cultural, eventos e novidades direto no seu e-mail."
            });

        // ---------- Configurações (guia, vídeo, SEO) ----------
        db.ConfiguracoesSite.AddRange(
            new ConfiguracaoSite { Chave = "site-titulo", Nome = "Título do site", Tipo = TipoConfiguracao.Texto, ValorTexto = "Descubra Estância — Capital Sergipana da Cultura" },
            new ConfiguracaoSite { Chave = "meta-descricao", Nome = "Meta descrição (SEO)", Tipo = TipoConfiguracao.Texto, ValorTexto = "Portal oficial de turismo de Estância/SE — Capital Sergipana da Cultura." },
            new ConfiguracaoSite { Chave = "guia-pdf", Nome = "Guia do turista", Tipo = TipoConfiguracao.Arquivo, ArquivoId = guia },
            new ConfiguracaoSite { Chave = "video-institucional", Nome = "Vídeo institucional", Tipo = TipoConfiguracao.Arquivo, ArquivoId = video },
            new ConfiguracaoSite { Chave = "logo", Nome = "Logotipo (rodapé)", Tipo = TipoConfiguracao.Arquivo, ArquivoId = logo });

        // ---------- Contatos do rodapé ----------
        db.Contatos.AddRange(
            new Contato { Tipo = TipoContato.Endereco, Rotulo = "Endereço", Valor = "Praça Barão de Estância, s/n — Centro\nCEP 49200-000 — Estância/SE", Icone = "map-pin", Ordem = 1 },
            new Contato { Tipo = TipoContato.Telefone, Rotulo = "Emergência", Valor = "190", Icone = "phone", Ordem = 2 },
            new Contato { Tipo = TipoContato.Telefone, Rotulo = "SAMU", Valor = "192", Icone = "ambulance", Ordem = 3 },
            new Contato { Tipo = TipoContato.Telefone, Rotulo = "SMTT", Valor = "(79) 3522-1500", Icone = "car", Ordem = 4 },
            new Contato { Tipo = TipoContato.Telefone, Rotulo = "CIT Turista", Valor = "(79) 3522-9090", Icone = "phone", Ordem = 5 },
            new Contato { Tipo = TipoContato.RedesSocial, Rotulo = "Instagram", Valor = "https://www.instagram.com/estanciaseoficial/", Icone = "instagram", Ordem = 6 },
            new Contato { Tipo = TipoContato.RedesSocial, Rotulo = "Facebook", Valor = "https://www.facebook.com/PrefeituraEstancia", Icone = "facebook", Ordem = 7 },
            new Contato { Tipo = TipoContato.RedesSocial, Rotulo = "YouTube", Valor = "https://www.youtube.com/@prefeituraestancia", Icone = "youtube", Ordem = 8 },
            new Contato { Tipo = TipoContato.RedesSocial, Rotulo = "WhatsApp", Valor = "https://wa.me/5579998765432", Icone = "message-circle", Ordem = 9 });

        // ---------- Eventos (datas relativas a hoje: a agenda do portal só
        // exibe eventos futuros, então tudo é ancorado em AddDays para o
        // seed demonstrar a seção com conteúdo). ----------
        db.Eventos.AddRange(
            new Evento
            {
                Titulo = "Festival do Barco de Fogo",
                Descricao = "A tradicional queima dos barcos de fogo ilumina o céu de Estância. Uma das festas mais aguardadas do Nordeste!",
                Local = "Orla da cidade, Estância/SE",
                DataInicio = DateTime.Today.AddDays(20).Date.AddHours(19),
                DataFim = DateTime.Today.AddDays(20).Date.AddHours(23).AddMinutes(59),
                Ordem = 1
            },
            new Evento
            {
                Titulo = "Concerto da Filarmônica Lira Carlos Gomes",
                Descricao = "Apresentação da centenária filarmônica no centro histórico da cidade.",
                Local = "Praça Barão do Rio Branco, Estância/SE",
                DataInicio = DateTime.Today.AddDays(14),
                DataFim = DateTime.Today.AddDays(14).AddHours(2),
                Ordem = 2
            },
            new Evento
            {
                Titulo = "São João de Estância",
                Descricao = "Forró, quadrilhas e comidas típicas na maior festa junina do litoral sergipano.",
                Local = "Praça de Eventos, Estância/SE",
                DataInicio = DateTime.Today.AddDays(45),
                DataFim = DateTime.Today.AddDays(45).AddDays(10),
                Ordem = 3
            });

        // ---------- Roteiros ----------
        var roteiro1 = new Roteiro
        {
            Titulo = "Roteiro 1 — História e Fé",
            Descricao = "Um passeio pela Estância antiga: da Catedral à Fábrica Velha, passando pela Lira Carlos Gomes.",
            Ordem = 1
        };
        var roteiro2 = new Roteiro
        {
            Titulo = "Roteiro 2 — Sol, Praia e Natureza",
            Descricao = "Dunas, mar calmo e águas cristalinas: um dia inteiro entre a Praia do Saco e a Lagoa dos Tambaquis.",
            Ordem = 2
        };
        db.Roteiros.AddRange(roteiro1, roteiro2);

        // Persiste pontos/roteiros para resolver os Ids antes de montar os itens.
        await db.SaveChangesAsync();

        var lira = await db.PontosTuristicos.FirstAsync(p => p.Nome == "Lira Carlos Gomes");
        var catedral = await db.PontosTuristicos.FirstAsync(p => p.Nome.Contains("Catedral"));
        var fabrica = await db.PontosTuristicos.FirstAsync(p => p.Nome.Contains("Fábrica"));
        var saco = await db.PontosTuristicos.FirstAsync(p => p.Nome == "Praia do Saco");
        var lagoa = await db.PontosTuristicos.FirstAsync(p => p.Nome.Contains("Lagoa"));

        db.RoteiroItens.AddRange(
            new RoteiroItem { RoteiroId = roteiro1.Id, PontoTuristicoId = catedral.Id, Dia = 1, Ordem = 1, Observacao = "Comece pela Catedral, marco religioso do século XVIII." },
            new RoteiroItem { RoteiroId = roteiro1.Id, PontoTuristicoId = lira.Id, Dia = 1, Ordem = 2, Observacao = "Ouça um ensaio da centenária filarmônica na praça." },
            new RoteiroItem { RoteiroId = roteiro1.Id, PontoTuristicoId = fabrica.Id, Dia = 1, Ordem = 3, Observacao = "Encerre visitando as ruínas da Fábrica Velha." },
            new RoteiroItem { RoteiroId = roteiro2.Id, PontoTuristicoId = saco.Id, Dia = 1, Ordem = 1, Observacao = "Manhã de banho de mar e caminhada nas dunas." },
            new RoteiroItem { RoteiroId = roteiro2.Id, PontoTuristicoId = lagoa.Id, Dia = 1, Ordem = 2, Observacao = "Tarde de interação com os tambaquis." });

        // ---------- Notícias ----------
        db.Noticias.AddRange(
            new Noticia
            {
                Titulo = "Estância celebra mais um Festival do Barco de Fogo",
                Resumo = "A Capital Nacional do Barco de Fogo reuniu milhares de visitantes em mais uma edição do espetáculo.",
                Corpo = "Estância reafirmou seu título de Capital Nacional do Barco de Fogo com uma edição memorável do festival. O céu da cidade se iluminou com os tradicionais artefatos pirotécnicos artesanais, em um espetáculo que encantou moradores e turistas.\n\nA programação contou com shows, feira de artesanato e a tradicional queima na orla. A expectativa da Secretaria de Turismo é que o evento movimente ainda mais o setor nos próximos anos.",
                DataPublicacao = DateTime.Today.AddDays(-10),
                Slug = "estancia-celebra-mais-um-festival-do-barco-de-fogo",
                Publicada = true,
                Ordem = 1
            },
            new Noticia
            {
                Titulo = "Lira Carlos Gomes abre temporada de concertos no centro histórico",
                Resumo = "A filarmônica centenária inicia programação de apresentações abertas ao público.",
                Corpo = "A Filarmônica Lira Carlos Gomes, fundada em 1879, abriu sua temporada de concertos com uma apresentação na Praça Barão do Rio Branco. O repertório incluiu clássicos da música popular brasileira e peças do cancioneiro sergipano.\n\nOs concertos acontecem mensalmente e são gratuitos. Acompanhe a agenda no portal.",
                DataPublicacao = DateTime.Today.AddDays(-4),
                Slug = "lira-carlos-gomes-abre-temporada-de-concertos",
                Publicada = true,
                Ordem = 2
            });

        // ---------- Avaliações (aprovadas para o modal) ----------
        db.Avaliacoes.AddRange(
            new Avaliacao { PontoTuristicoId = saco.Id, Nome = "Mariana Souza", Nota = 5, Comentario = "Praia linda, mar calmo e pôr do sol inesquecível. Vale muito a pena!", Aprovada = true },
            new Avaliacao { PontoTuristicoId = lira.Id, Nome = "João Pedro", Nota = 5, Comentario = "A música da filarmônica na praça é emocionante. História viva!", Aprovada = true },
            new Avaliacao { PontoTuristicoId = lagoa.Id, Nome = "Ana Clara", Nota = 4, Comentario = "Experiência única com os tambaquis. Leve chinelo e protetor solar.", Aprovada = true },
            new Avaliacao { PontoTuristicoId = saco.Id, Nome = "Carlos Andrade", Nota = 5, Comentario = "Melhor praia de Sergipe! Água cristalina e infraestrutura boa.", Aprovada = false });

        await db.SaveChangesAsync();
        Console.WriteLine("[seed] Dados do protótipo importados com sucesso.");
    }

    private static async Task GarantirAdminAsync(UserManager<Usuario> userManager)
    {
        if (await userManager.Users.AnyAsync()) return;

        var admin = new Usuario
        {
            UserName = "admin@estancia.se.gov.br",
            Email = "admin@estancia.se.gov.br",
            NomeCompleto = "Administrador do Portal",
            EmailConfirmed = true
        };

        var senha = "Estancia@2026";
        var resultado = await userManager.CreateAsync(admin, senha);
        if (resultado.Succeeded)
        {
            await userManager.AddClaimAsync(admin, new System.Security.Claims.Claim("Perfil", "Gerenciador"));
            Console.WriteLine($"[seed] Usuário admin criado: admin@estancia.se.gov.br / {senha}");
        }
        else
        {
            Console.WriteLine($"[seed] Falha ao criar admin: {string.Join("; ", resultado.Errors.Select(e => e.Description))}");
        }
    }

    // ---------- Helpers ----------

    private static CategoriaPontoTuristico AddCategoria(AppDbContext db, string chave, string nome, string? sub, string? cor, string? icone, int ordem, bool maravilhas = true)
    {
        var c = new CategoriaPontoTuristico
        {
            Chave = chave,
            Nome = nome,
            SubTitulo = sub,
            Cor = cor,
            Icone = icone,
            Ordem = ordem,
            ApresentarEmMaravilhas = maravilhas,
            ExibirNoMapa = true
        };
        db.CategoriasPontosTuristicos.Add(c);
        return c;
    }

    private static void AddPonto(
        AppDbContext db, CategoriaPontoTuristico cat, string nome, string? desc, string? detalhe,
        string? tag, string? icone, int left, int top, int ordem, string? endereco, string? comoChegar,
        long? capaId, long? pictoId, bool exibirMapa = true)
    {
        var ponto = new PontoTuristico
        {
            CategoriaId = cat.Id,
            Nome = nome,
            Descricao = desc,
            Detalhe = detalhe,
            Tag = tag,
            Icone = icone,
            LeftPercent = left,
            TopPercent = top,
            Ordem = ordem,
            Endereco = endereco,
            ComoChegar = comoChegar,
            ExibirNoMapa = exibirMapa,
            Ativo = true
        };
        db.PontosTuristicos.Add(ponto);

        // Mídias via navigation property: o EF resolve as FKs no SaveChanges.
        var ordemMidia = 0;
        if (capaId is long cid)
        {
            ponto.Midias.Add(new PontoTuristicoMidia { ArquivoId = cid, Tipo = TipoMidia.Capa, Ordem = ++ordemMidia });
        }
        if (pictoId is long pid)
        {
            ponto.Midias.Add(new PontoTuristicoMidia { ArquivoId = pid, Tipo = TipoMidia.Pictograma, Ordem = ++ordemMidia });
        }
    }

    private static async Task<Dictionary<string, long>> SalvarCapasAsync(AppDbContext db)
    {
        var mapas = new (string Chave, string Arquivo)[]
        {
            ("lira", "lira carlos gomes.jpeg"),
            ("igreja", "igreja.jpeg"),
            ("fabrica", "fabrica.jpeg"),
            ("barco", "barcos de fogo.jpeg"),
            ("lagoa", "lagoa dos tambaquis.jpeg"),
            ("saco", "praia do saco.jpeg"),
            ("cristo", "cristo.jpeg")
        };

        var resultado = new Dictionary<string, long>();
        foreach (var (chave, arquivo) in mapas)
        {
            resultado[chave] = await SalvarArquivoAsync(db, arquivo, "image/jpeg");
        }
        return resultado;
    }

    private static async Task<Dictionary<string, long>> SalvarPictogramasAsync(AppDbContext db)
    {
        var mapas = new (string Chave, string Arquivo)[]
        {
            ("lira", "pictogramas lira carlos gomes.png"),
            ("igreja", "pictogramas igreja.png"),
            ("fabrica", "pictogramas fabrica.png"),
            ("barco", "pictogramas barco de fogo.png"),
            ("lagoa", "pictogramas lagoa.png"),
            ("saco", "pictogramas praia do saco.png"),
            ("cristo", "pictogramas cristo.png")
        };

        var resultado = new Dictionary<string, long>();
        foreach (var (chave, arquivo) in mapas)
        {
            resultado[chave] = await SalvarArquivoAsync(db, arquivo, "image/png");
        }
        return resultado;
    }

    private static async Task<long> SalvarArquivoAsync(AppDbContext db, string nomeArquivo, string contentType)
    {
        var caminho = Path.Combine(ImgPath, nomeArquivo);
        if (!File.Exists(caminho))
        {
            throw new FileNotFoundException($"Mídia do protótipo não encontrada: {caminho}");
        }

        var arquivo = new Arquivo
        {
            UID = Guid.NewGuid(),
            Nome = nomeArquivo,
            ContentType = contentType,
            Size = new FileInfo(caminho).Length,
            Bytes = await File.ReadAllBytesAsync(caminho),
            Origem = "seed",
            Ativo = true
        };
        db.Arquivos.Add(arquivo);

        // Persiste já para obter o Id real (o restante do seed referencia por FK).
        await db.SaveChangesAsync();
        return arquivo.Id;
    }
}
