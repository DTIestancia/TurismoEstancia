# 🛠️ Manual Técnico — TurismoEstância

Documento de manutenção do portal de turismo de Estância/SE. Foco em **arquitetura,
fluxos e como evoluir o sistema com segurança**. O projeto segue o padrão genérico do
[`PADRAO-DE-PROJETO.md`](../PADRAO-DE-PROJETO.md) (Prefeitura Digital) — este arquivo é
a **visão concreta** do TurismoEstância.

---

## 1. Stack e visão geral

| Item | Tecnologia |
| --- | --- |
| Plataforma | ASP.NET Core **9** (MVC + Razor Pages + Áreas) |
| ORM | Entity Framework Core **9** (SQL Server) |
| Bancos | `TurismoEstanciaDb` (domínio) + `TurismoEstanciaIdentityDb` (Identity) |
| Front-end | Razor + SCSS (compilado no build) + JS vanilla (`wwwroot/js/portal.js`) + Leaflet (mapa) + Lucide (ícones) |
| Autenticação | ASP.NET Identity com **policies por claim** |
| Mídias | `byte[]` na tabela `Arquivos` (pronta para FILESTREAM) |

Fluxo de dependências (sempre "para baixo"):

```
TurismoEstancia.Web ──► Services ──► Domain
        │   └────────► Domain
        ├──────────► Authorization
        ├──────────► Mail      (reservado)
        └──────────► Identity  (folha)
```

**Regra de ouro:** o `Web` não contém regra de negócio. Tudo vive nas libraries;
`Domain` não referencia nada além do EF Core.

---

## 2. Estrutura da solução

```
TurismoEstancia.slnx
├── TurismoEstancia.Web/                # Entrada (MVC + Razor Pages + Áreas)
│   ├── Program.cs                      # Fino: encadeia as Extensions
│   ├── Extensions/                     # Composição (DI, DB, Identity, Pipeline)
│   ├── Areas/Gerenciador/              # CMS: "Conteúdo do site" (10 áreas, SecoesController)
│   │                                   #   + CRUDs + Tema/Configurações/Newsletter/Avaliações
│   ├── Areas/Operador/                 # Eventos + Newsletter (perfil restrito)
│   ├── Components/                     # ViewComponents: LogoSite, ThemeSite, ContatosRodape
│   ├── Controllers/                    # Portal público e endpoints
│   ├── Infrastructure/SeoService.cs    # SEO por página (title/meta/OG/Twitter)
│   ├── Infrastructure/ConfiguracaoSiteCache.cs  # decorator: 1 consulta de config por request
│   ├── Middleware/AnalyticsVisitTrackingMiddleware.cs
│   ├── Middleware/FaviconMiddleware.cs # favicon dinâmico (logo-principal ou estático)
│   ├── Models/                         # ViewModels do portal (Home, Paginas...) e das áreas
│   ├── Pages/Noticias|Roteiros/        # Razor Pages públicas
│   ├── Views/Home/                     # 11 partials do portal (hero, história...)
│   └── wwwroot/scss/                   # SCSS: _portal.scss (verbatim) + main.scss (ajustes)
├── TurismoEstancia.Domain/             # Entidades + DbContext + DTOs + migrações
├── TurismoEstancia.Services/           # 7 módulos: Turismo, CulturaGastronomia, Roteiro,
│                                       #   Conteudo, Comunicacao, Avaliacao, Analytics (+Infra)
├── TurismoEstancia.Authorization/      # ClaimObrigatoria + handler de policies
├── TurismoEstancia.Identity/           # Usuario + IdentityContext (banco próprio)
└── TurismoEstancia.Mail/               # (reservado) e-mail
```

### Ponto de entrada (Program.cs fino)

```csharp
builder.AddDatabase();          // DbContexts (2 connection strings)
builder.AddIdentityConfig();    // Identity, cookies, policies
builder.AddBusinessServices();  // todos os services (AddScoped)
builder.AddInfrastructure();    // MVC, Razor Pages, SeoService, upload limit...

app.UseStandardPipeline();      // pt-BR, static files, auth, analytics middleware
app.MapAllRoutes();             // áreas + rota padrão + Razor Pages
await app.RunAsync();           // (sem --seed: seed é proibido por regra)
```

---

## 3. Domain — entidades e DbContext

Entidades (`Domain/Models/`): `Arquivo`, `Slide`, `Estatistica`, `CategoriaPontoTuristico`,
`PontoTuristico`, `PontoTuristicoMidia`, `HorarioFuncionamento`, `Avaliacao`, `Evento`,
`GrupoCultural`, `PratoTuristico`, `TagCultural`, `ConteudoSite`, `ConfiguracaoSite`,
`Contato`, `Noticia`, `Roteiro`, `RoteiroItem`, `InscricaoNewsletter`, `AnalyticsEvento`.

Convenções de configuração no `AppDbContext.OnModelCreating`:

- **Datas**: `HasDefaultValueSql("GETDATE()")`.
- **Bools**: `HasDefaultValue(true)`.
- **FKs**: `Restrict`/`SetNull` — **Cascade apenas em filhos próprios** (ex.: mídias do ponto).
- **Enums**: `HasConversion<string>()` (armazenados como string).
- **Índices únicos filtrados**: `HasFilter` para unicidade condicional.
- Leituras sempre `AsNoTracking()` + projeção `ToDto` (Expression estática).

### Comandos de migração

```bash
# O startup project é SEMPRE o Web
dotnet ef migrations add <Nome> --project TurismoEstancia.Domain --startup-project TurismoEstancia.Web --context AppDbContext
dotnet ef database update       --project TurismoEstancia.Domain --startup-project TurismoEstancia.Web --context AppDbContext
```

> Nunca mexa em migrações do `IdentityContext` (banco de usuários, intacto).

### Seed — REMOVIDO (regra permanente)

> ⛔ **Proibido:** o `DatabaseSeeder` e o suporte a `--seed` foram **removidos do
> projeto** por regra permanente. **NUNCA** reintroduzir seed nem usar `HasData`
> (senão `database update` semearia dados em produção).

Dados de referência entram por **migração de schema**; os acessos ao painel são
provisionados pelo **gerenciador geral de acessos da DTI** (sistemas do município) —
este projeto não cria nem gerencia usuários (módulo Usuários removido; banco do
Identity intocável).

---

## 4. Services — padrão de módulo

Cada módulo em `Services/{Modulo}/` tem `Interfaces/INomeService.cs` +
`Services/NomeService.cs`, registrado com `AddScoped` em
`Extensions/BusinessServiceExtensions.cs` (comentário `// Novos módulos serão adicionados aqui`).

Módulos atuais: **Turismo** (pontos, categorias, mídias, horários, avaliações),
**CulturaGastronomia** (grupos, pratos, tags), **Roteiro** (roteiros/notícias),
**Conteudo** (conteúdos, configurações, slides, estatísticas, contatos, notícias),
**Comunicacao** (newsletter), **Avaliacao**, **Analytics**.

> **Cache de configurações:** o `IConfiguracaoSiteService` público é um decorator
> (`Web/Infrastructure/ConfiguracaoSiteCache`) que carrega todas as configurações
> **1 vez por request** — o SEO, o header, o rodapé, o favicon e os controllers
> leem do dicionário (1 consulta por página). Sempre fresco (cache morre no
> request) e invalidado ao salvar/excluir configuração. Se o painel ler dado
> velho após salvar, é bug — a invalidação roda junto com o `SalvarAsync`/`ExcluirAsync`.

### Como adicionar um módulo novo (8 passos)

1. **Entidade** em `Domain/Models/` (+ DbSet + config fluente no `AppDbContext`).
2. **DTO** em `Domain/DTOs/`.
3. **Interface + Service** em `Services/{Modulo}/` (projeção `ToDto`, `AsNoTracking`,
   `CancellationToken`, `InvalidOperationException` pt-BR para "não encontrado").
4. **Registro DI** em `BusinessServiceExtensions` (`AddScoped`).
5. **Controller** na área certa com `[Authorize(Policy = "Gerenciador")]`.
6. **Mapper/ViewModel** (`Areas/Gerenciador/Models/` ou `Web/Models/`).
7. **Views** (Index + Formulario).
8. **Migração de schema** (dados de referência entram por migração — **seed é proibido** por regra).

> Para um módulo visível no **portal**, o passo extra é: service consumido no
> `HomeController`/`PaginasController` → partial nova em `Views/Home/` → classes de
> layout/estilo em `main.scss`.

---

## 5. Mídias e FILESTREAM

- Uploads vão para **`byte[]` na tabela `Arquivos`** (padrão `PrefeituraDigital.Arquivo`:
  `ArquId`, `ArquUID` ROWGUIDCOL, `ArquFileName`, `ArquContentType`, `ArquSize`,
  `ArquBytes varbinary(max)`, `ArquMomento`, `ArquAutor`, `ArquAtivo`, `ArquOrigem`).
- Servidas por **`GET /arquivo/{id}`** (`ArquivoController`) com Content-Type correto.
- **Otimização de imagem** (`IArquivoService.SalvarImagemOtimizadaAsync` /
  `SalvarThumbnailAsync`, **SixLabors.ImageSharp 3.1**): redimensiona para o máximo
  indicado (1600px galeria / 400px thumbnail), **re-encoda JPEG** (qualidade 82/75,
  `SkipMetadata` remove **EXIF/GPS** — LGPD) e grava na `Arquivos`; só reduz, nunca
  amplia. Usada pela **Galeria** (2 registros por foto: imagem cheia + thumbnail).
- **Marca d'água** (`comMarcaDagua: true` na otimização): listras diagonais sutis
  (padrão gerado em baixa resolução + resize bilinear + alfa) + **logotipo do portal**
  (configuração `logo-principal`) no canto inferior direito — só com o core do
  ImageSharp, sem dependência extra. Falha silenciosamente (nunca derruba upload).
- **Marca d'água no visualizador** (partial `_LightboxGaleria`, usado em `/galeria` e
  na faixa da notícia): o lightbox exibe sobre a foto (canto inferior esquerdo) um
  selo com câmera + "Foto: Portal de Turismo de Estância · Capital Sergipana da
  Cultura" linkando para a raiz do site (`data-track="marca-dagua"`). A marca é
  overlay CSS — vale para qualquer imagem aberta no visualizador, inclusive as de
  notícias (que não recebem a marca embutida no upload).
- **Proteção contra hotlink** (`ArquivoController`): `Referer` de host diferente do
  site → **403**; acesso sem Referer (nova aba, OG/redes sociais, crawlers) e do
  próprio site continua liberado.
- **Cache HTTP** (`ArquivoController`): os arquivos da tabela são imutáveis (upload
  sempre cria um registro novo), então o `GET /arquivo/{id}` envia
  `Cache-Control: public, max-age=31536000, immutable` para imagens (1 ano, sem
  revalidação) e `max-age=604800` (7 dias) para as demais mídias + `ETag`
  (`id-CriadoEm`) com suporte a `If-None-Match` → **304** e range requests. O 403
  do hotlink nunca recebe cache.
- **Galeria**: `GaleriaCategorias` (chave única → `/galeria/{chave}`, `CapaArquivoId`
  opcional com FK `SetNull` — capa otimizada usada no card da categoria e OG/SEO) +
  `GaleriaMidias` (**tabela de vínculo muitos-para-muitos**: a foto é o par
  `ArquivoId`/`ArquivoThumbId` na tabela `Arquivos`, compartilhado entre categorias;
  índice único `(CategoriaId, ArquivoId)` impede duplicar a mesma foto na mesma
  categoria; FK Cascade p/ categoria, **Restrict** p/ `Arquivos`; `Ordem`/`Ativo`
  por categoria, `Titulo` por vínculo). **Vincular fotos existentes** cria apenas o
  vínculo (sem binário novo); excluir de uma categoria só apaga os binários se
  nenhuma outra categoria referenciá-los (`EstaReferenciadoAsync`). Módulo
  `Services/Galeria`; admin em `Areas/Gerenciador/Controllers/GaleriaController`;
  portal em `Controllers/GaleriaController` (lightbox em `portal.js`,
  `initGaleriaLightbox`; visão "Todas" deduplica a foto por `ArquivoId`).
  **Lazy-load + placeholder**: as imagens usam `loading="lazy"` + `decoding="async"`
  e começam transparentes sobre um **shimmer animado** (CSS `galeria-shimmer`); o JS
  (`initGaleriaPlaceholder`) marca `.is-carregada` no evento `load` (fade-in, sem
  layout shift — o `height` fixo reserva o espaço) e `.is-erro` se a imagem falhar.
  O lightbox aplica o mesmo fade-in (`is-loaded`) a cada troca de foto.
- Entidades guardam `long? XxxArquivoId` com FK `SetNull`.
- **FILESTREAM**: a estrutura já está pronta. Quando o filegroup existir no servidor,
  execute [`Deploy/01-Filestream-Config.sql`](../Deploy/01-Filestream-Config.sql) para
  converter `ArquBytes` em `varbinary(max) FILESTREAM` — **zero mudança de código** (o EF
  lê/grava o binário igual).

---

## 6. Autenticação e autorização

- **Identity em banco separado** (`IdentityContext`), cookie `HttpOnly`, lockout 5/5min,
  senha forte, `RequireConfirmedEmail`.
- **Policies por claim** (nunca roles literais no `[Authorize]`):
  `TurismoEstancia.Authorization` define `ClaimObrigatoria` + handler; as policies
  `Gerenciador` e `Operador` são registradas em `IdentityExtensions`.
- `[Authorize(Policy = "Gerenciador")]` nos controllers do CMS;
  `[Authorize(Policy = "Operador")]` na área Operador (Eventos + Newsletter).
- **Não há seed nem admin local**: os acessos ao painel (claims `Gerenciador`/`Operador`)
  são provisionados pelo **gerenciador geral de acessos da DTI** (sistemas do município).

---

## 7. Analytics (visitas e cliques)

Fluxo anônimo (sem dados pessoais):

1. **`AnalyticsVisitTrackingMiddleware`** — a cada requisição do portal cria/lê o cookie
   `te_sessao`, detecta dispositivo (Desktop/Mobile/Tablet via User-Agent) e grava
   `AnalyticsEvento` (Tipo = "Visita", rota, referer, data).
2. **Beacon de cliques** — o `portal.js` envia `POST /api/analytics/event` via
   `sendBeacon` (não bloqueia navegação) com `data-track`/`data-track-id` dos elementos.
3. **Dashboard** (`Gerenciador/DashboardController`) — `IAnalyticsService.ObterResumoAsync`
   agrega por período (7/30/90 dias): visitas por dia/rota, cliques por evento,
   top rotas/entidades, fontes (referer) e dispositivos.

> Eventos rastreados hoje: `ver-maravilha` (botão da vitrine), `roteiro` (cards de
> roteiro) e `noticia` (cards de notícia), `visualizacao-foto`/`like-foto` (galeria).
> **Qualquer elemento com atributo `data-track`** (+ opcionais `data-track-id`/`data-track-nome`)
> é rastreado automaticamente pelo `portal.js` — basta adicionar o atributo em
> novos links/botões para começar a medir cliques.

**Engajamento da galeria:** `GaleriaMidias` tem colunas `Visualizacoes`/`Curtidas`
(int, default 0), incrementadas pelos endpoints `POST /galeria/visualizar/{id}` e
`POST /galeria/curtir/{id}` (curtida com **dedup por sessão**: consulta `AnalyticsEvento`
com `Evento = "like-foto"` + `EntidadeId` + `SessaoId` antes de incrementar). O Dashboard
exibe **"Fotos mais visualizadas/curtidas"** no período, derivados dos eventos
`visualizacao-foto`/`like-foto`. O ranking aceita **filtro por categoria**
(`?galeriaCategoria=` no Dashboard — select que preserva o período `dias`): quando
selecionado, `ObterResumoAsync` junta os eventos com `GaleriaMidias` (o evento guarda
o Id do vínculo) e restringe o ranking à categoria (o filtro lista categorias ativas
e inativas, para rankings antigos continuarem consultáveis).

---

## 8. SEO

- **`SeoService`** (`Web/Infrastructure/`) — resolve `title`, `meta description`,
  Open Graph e Twitter Cards por página a partir da rota e das Configurações do CMS
  (`site-titulo`, `meta-descricao`, imagem de compartilhamento).
- **Sitemap** dinâmico (`SeoController`) lista as rotas públicas estáticas + detalhes
  do banco (maravilhas, notícias, roteiros, grupos, pratos, tags).
- **`noindex`** configurável por página.
- O `_Layout.cshtml` injeta as meta tags; `ViewData["Seo"]` permite sobrepor por página.
- **Favicon dinâmico** (`FaviconMiddleware`, antes do `UseStaticFiles`): o
  `/favicon.ico` redireciona (302, cache 1h) para a imagem de `logo-principal`
  quando configurada; sem ela, o `wwwroot/favicon.ico` estático é servido.
  O `<link rel="icon" href="/favicon.ico" />` está no `_Layout`, no
  `_PainelLayout` e no `Login`.

---

## 9. Portal público — rotas e views

**Controllers públicos** (`Controllers/`):

| Rota | Controller/Action |
| --- | --- |
| `/` | `HomeController.Index` (home com 11 partials) |
| `/cidade`, `/cultura`, `/grupos-populares`, `/gastronomia`, `/lugares` | `PaginasController` |
| `/lugares/{id}/{slug}`, `/grupos-populares/{id}/{slug}`, `/gastronomia/{id}/{slug}`, `/cultura/{id}/{slug}` | detalhes |
| `/noticias`, `/noticias/{slug}` e `/roteiros`, `/roteiros/{slug}` | Razor Pages (`Pages/`) |
| `/galeria`, `/galeria/{chave}` | `GaleriaController` (galeria de fotos por categoria) |
| `POST /galeria/visualizar/{id}` | contabiliza visualização (lightbox) + evento `visualizacao-foto` |
| `POST /galeria/curtir/{id}` | curtida "Amei" com dedup por sessão + evento `like-foto` |
| `/arquivo/{id}` | download de mídia (bloqueia hotlink via Referer) |
| `/evento/{id}.ics` | agenda → calendário |
| `/api/analytics/event` | beacon de cliques |

**Partials da home** (`Views/Home/`): `_Hero` (navbar, preloader, hero),
`_Historia`, `_Cultura`, `_Gastronomia`, `_Maravilhas` (vitrine),
`_Agenda`, `_Mapa`, `_Roteiros`, `_Noticias`, `_Video`, `_Footer`, `Privacy`.

**Vitrine das 7 Maravilhas** — `Views/Shared/_VitrineMaravilhas.cshtml`, JS em
`portal.js` (`initWondersVitrine`): **baralho de cartas com prévia** via scroll-snap
horizontal — a carta mais próxima do centro do deck é a atual (`is-atual`; escala 1,
nítida), a próxima espreita à direita e a anterior fica à esquerda. Navegação por
**setas ‹ › sobrepostas no centro da foto**, **teclado ← →**, **clique na carta
lateral** (avança/volta) e **arrasto** (mouse: snap desativado durante o arrasto via
`.is-dragging` e re-encaixe ao soltar; toque: scroll nativo). Contador
`#vitrineCounter` ("Maravilha X de 7"). As extremidades do deck são centralizadas
com espaçadores `::before/::after` (o `%` do flex-basis e o padding resolvem contra
boxes diferentes). **`/lugares` é uma listagem** (grid `paginas-card--maravilha`
numerada 01–07) — não usa o baralho.

**Notícias** — `NoticiaService` + `Pages/Noticias` (`Index`/`Detalhe`). A notícia
pode **vincular uma galeria já salva** (`Noticia.GaleriaCategoriaId` → `GaleriaCategorias`,
SetNull, migração `AdicionaGaleriaNoticia`): o formulário do Gerenciador tem o
select "Galeria relacionada" e a página de detalhe renderiza a faixa com **no
máximo 6 fotos** (a 6ª ganha overlay "+N") + botão "Ver todas as fotos (N)"; o
clique em qualquer foto, no overlay ou no botão abre o **lightbox** (mesmo da
`/galeria`) que navega por **todas** as fotos — com curtida e contagem de
visualização — sem sair da página. A **imagem de capa tem recorte ajustável** no
Gerenciador (`_AjusteImagem` partial): sliders de **zoom (100–250%)**, **posição
vertical e horizontal (0–100%)** com prévia ao vivo em janela 16:9; os valores
são gravados em `Noticia.ImagemZoom/ImagemPosicaoX/ImagemPosicaoY` (migração
`AdicionaAjusteImagemNoticia`) e aplicados no portal via `object-position` +
`transform: scale(var(--noticia-zoom))` com `transform-origin` no foco — no
detalhe (wrapper 16:9 com `overflow:hidden`), nos cards do home (`ig-post-img`)
e na listagem (`news-card-img-wrap`). O `Corpo` é editado com o **editor rico
Quill 2** (`_EditorTextoRico` partial, lib em `wwwroot/lib/quill/`) —
negrito/itálico/sublinhado/tachado, **tamanho de fonte** (12–36px via style
inline), **cor da paleta** (hex do tema), listas, link e citação; o HTML é
sincronizado para o textarea oculto no submit. Ajuste no DTO: `Slug` é anulável
(gerado no servidor no cadastro — evita o `[Required]` implícito de tipos não
anuláveis que quebrava o Criar).

**Contatos do rodapé** — `Components/ContatosRodapeViewComponent` + view `Default`
(com **cache por request** via `HttpContext.Items` — a home invoca o componente 3×
mas faz 1 consulta): endereços com `map-pin`, telefones com ícone por contato e
**detecção de WhatsApp** (`wa.me`/`whatsapp` → link `https://wa.me/<nº>`), redes
sociais com **detecção pela URL** (instagram/facebook/youtube/tiktok/x/linkedin)
e **e-mails** (`TipoContato.Email` → ícone de envelope `mail` + link `mailto:`).
Usado no `_Footer` (home) e no `_PaginasFooter` (páginas internas) — única fonte de
verdade para a marcação dos contatos.

**Logotipo e tema dinâmicos** — `Components/LogoSiteViewComponent` (lê `logo-principal`,
renderiza `/arquivo/{id}`, fallback para a assinatura; usado no navbar, headers,
login e `_PainelLayout`; cache por request) e `Components/ThemeSiteViewComponent`
(emite `<style id="tema-site">` com as 6 cores da paleta editáveis no Gerenciador
`TemaController`, chaves `tema-cor-*`; vale no portal, painel e login).

### Gerenciador — "Conteúdo do site" (áreas)

O `SecoesController` (`Areas/Gerenciador/Controllers/`) é o hub de edição das **10
áreas** (Hero, Nossa Cidade, Cultura, Gastronomia, 7 Maravilhas, Agenda, Notícias,
Roteiros, Mapa, Rodapé). Cada ação monta um `AreaSiteViewModel` com:

- **Itens cadastrados** (cards `ItensAreaViewModel` — o que a área já tem) + botão
  "Cadastrar novo" que abre o **modal de cadastro** do CRUD correspondente
  (`dialog` com fade+escala, `FecharDialogPainel` no `painel.js`) e "Ver todos"
  (lista completa em nova aba);
- **Textos** da área (chave → valor, com dica e `PrevisualizarTexto` ao vivo —
  textarea simples; o **editor rico é exclusivo das notícias**);
- **Imagens** da área (upload substitui a atual);
- **Slides do hero** (gestão inline na área Hero) e **estatísticas** (chips na área
  Cidade);
- Subnav `_SubnavAreas` (10 áreas + **"Ver seção no portal"** com âncora e
  **"Prévia"** em iframe) — reutilizada nos CRUDs relacionados (Categorias, Grupos,
  Eventos...).

O **seletor de chave** (`_SeletorChave`) substitui a digitação manual de chaves:
lista o catálogo disponível (por contexto) e **desabilita chaves já em uso** — usado
em Conteúdos, Configurações, Categorias, Estatísticas e Contatos.

---

## 10. SCSS (estilo)

- **`_portal.scss`** — CSS do protótipo portado **verbatim** (tokens `:root`,
  todas as classes originais). **Não editar regras aqui** (não reverter correções do
  protótipo); mudanças vão no `main.scss`.
- **`_tokens.scss`** — tokens reutilizáveis (`$shadow-card`, etc.).
- **`main.scss`** — `@use 'portal'` + **ajustes dinâmicos e overrides** (preloader,
  baralho das 7 Maravilhas, `--secao-cor` por seção, páginas internas, responsividade
  mobile). Por vir **depois** no cascade, um override de mesma especificidade vence.
- **Paleta editável** — o `ThemeSiteViewComponent` emite um `<style id="tema-site">`
  que sobrescreve as variáveis de cor em runtime (sem recompilar): as seções usam
  `var(--secao-cor)`/`var(--tema-*)` e as 6 cores oficiais viram `var(--cor-*)`
  quando o tema personalizado está ativo.
- Compilado no **build** (`sasscompiler.json`, pacote `AspNetCore.SassCompiler`) para
  `wwwroot/css/main.css`.

**Convenções mobile** (o portal é mobile-first — público principal):
- Breakpoints usados: `1024` (menu/nav), `900` (vitrine), `860` (páginas internas),
  `640` (paddings/grids 1fr), `480` (navbar/hero/vitrine compactos).
- Grids com `minmax(320px, 1fr)` **devem** ter override `1fr` em `≤640px` (senão
  estouram em telas ≤ 368px).

---

## 11. Deploy

- **Pré-requisitos**: .NET 9 runtime, SQL Server, `ASPNETCORE_ENVIRONMENT=Production`.
- **Não existe seed** — proibido por regra (o `DatabaseSeeder` foi removido).
- **Banco**: aplicar migrações (`dotnet ef database update`) e, para mídias grandes,
  seguir [`Deploy/01-Filestream-Config.sql`](../Deploy/01-Filestream-Config.sql).
- **Segurança**: usar connection strings por secrets/env vars, HTTPS obrigatório (o
  `_Layout` gera canônicos/OG com `Request.Scheme`); acessos ao painel são da DTI
  (não há admin local para "trocar senha").

---

## 12. Troubleshooting técnico

| Sintoma | Causa / solução |
| --- | --- |
| `500` na home com `Index was out of range` em `_Historia.cshtml` | Guard de slides com off-by-one: a seção exige **3+ slides** (`Slides[1]`/`[2]`). Corrigido para `Count >= 3`; garanta 3+ slides no CMS. |
| CSS antigo servido | O SCSS compila no build: rode `dotnet build` e reinicie o servidor; o `asp-append-version` quebra cache. |
| Porta ocupada | Porta padrão `5257` (`launchSettings.json`, perfil `http`); troque no perfil ou use `--urls`. |
| Erro JS "navbar is null" | Guarda `if (!navbar) return;` em `updateNavbar` (páginas internas não têm `#navbar`) — não remover. |
| Analytics vazio | Sem cookie `te_sessao` (aba anônima/bloqueada) ou navegação sem o middleware. |
| Pictogramas brancos no preloader | Comportamento esperado (`brightness(0) invert(1)`); se quiser cor, sobrescreva `.loader-picto` no `main.scss`. |
| Migração pendente no Identity | Nunca rode `database update` do IdentityContext manualmente; o app aplica no startup (Development). |

---

## 13. Comandos úteis

```bash
# Build da solução (0 erros / 0 avisos é o padrão aceito)
dotnet build TurismoEstancia.slnx

# Rodar o portal (porta 5257)
dotnet run --project TurismoEstancia.Web --launch-profile http

# Migração nova (Domain)
dotnet ef migrations add <Nome> --project TurismoEstancia.Domain --startup-project TurismoEstancia.Web --context AppDbContext

# Aplicar migrações
dotnet ef database update --project TurismoEstancia.Domain --startup-project TurismoEstancia.Web --context AppDbContext
```

---

## 14. Notas de evolução (roadmap técnico)

- **Itens de roteiro** editáveis no CMS (hoje são dados de referência no banco — ver
  `RoteiroItem`).
- **Paginação** em Newsletter/Avaliações no painel.
- **Testes automatizados** (xUnit) para services e controllers — hoje a validação é
  build + smoke manual.
- **Redefinição de senha** por e-mail (`TurismoEstancia.Mail` está reservado para isso).
- **API/mobile** (`.Api` + `.MAUI`) seguindo o `PADRAO-DE-PROJETO.md`, se surgir demanda.
