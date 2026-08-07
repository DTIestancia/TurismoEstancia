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
│   ├── Extensions/                     # Composição (DI, DB, Identity, Pipeline, seed)
│   ├── Areas/Gerenciador/              # CMS completo (16 CRUDs + Dashboard, além do PainelController base)
│   ├── Areas/Operador/                 # Eventos + Newsletter (perfil restrito)
│   ├── Controllers/                    # Portal público e endpoints
│   ├── Infrastructure/SeoService.cs    # SEO por página (title/meta/OG/Twitter)
│   ├── Middleware/AnalyticsVisitTrackingMiddleware.cs
│   ├── Models/                         # ViewModels do portal (Home, Paginas...)
│   ├── Pages/Noticias|Roteiros/        # Razor Pages públicas
│   ├── Views/Home/                     # 12 partials do portal (hero, história...)
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

### Como adicionar um módulo novo (8 passos)

1. **Entidade** em `Domain/Models/` (+ DbSet + config fluente no `AppDbContext`).
2. **DTO** em `Domain/DTOs/`.
3. **Interface + Service** em `Services/{Modulo}/` (projeção `ToDto`, `AsNoTracking`,
   `CancellationToken`, `InvalidOperationException` pt-BR para "não encontrado").
4. **Registro DI** em `BusinessServiceExtensions` (`AddScoped`).
5. **Controller** na área certa com `[Authorize(Policy = "Gerenciador")]`.
6. **Mapper/ViewModel** (`Areas/Gerenciador/Models/` ou `Web/Models/`).
7. **Views** (Index + Formulario).
8. **Migração + seed** idempotente (se houver dados de referência).

> Para um módulo visível no **portal**, o passo extra é: service consumido no
> `HomeController`/`PaginasController` → partial nova em `Views/Home/` → classes de
> layout/estilo em `main.scss`.

---

## 5. Mídias e FILESTREAM

- Uploads vão para **`byte[]` na tabela `Arquivos`** (padrão `PrefeituraDigital.Arquivo`:
  `ArquId`, `ArquUID` ROWGUIDCOL, `ArquFileName`, `ArquContentType`, `ArquSize`,
  `ArquBytes varbinary(max)`, `ArquMomento`, `ArquAutor`, `ArquAtivo`, `ArquOrigem`).
- Servidas por **`GET /arquivo/{id}`** (`ArquivoController`) com Content-Type correto.
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
- **Usuário admin** criado pelo seed com as duas claims.

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
> roteiro) e `noticia` (cards de notícia). **Qualquer elemento com atributo
> `data-track`** (+ opcionais `data-track-id`/`data-track-nome`) é rastreado
> automaticamente pelo `portal.js` (linha ~803) — basta adicionar o atributo em
> novos links/botões para começar a medir cliques.

---

## 8. SEO

- **`SeoService`** (`Web/Infrastructure/`) — resolve `title`, `meta description`,
  Open Graph e Twitter Cards por página a partir da rota e das Configurações do CMS
  (`site-titulo`, `meta-descricao`, imagem de compartilhamento).
- **Sitemap** dinâmico (`SeoController`) lista as rotas públicas estáticas + detalhes
  do banco (maravilhas, notícias, roteiros, grupos, pratos, tags).
- **`noindex`** configurável por página.
- O `_Layout.cshtml` injeta as meta tags; `ViewData["Seo"]` permite sobrepor por página.

---

## 9. Portal público — rotas e views

**Controllers públicos** (`Controllers/`):

| Rota | Controller/Action |
| --- | --- |
| `/` | `HomeController.Index` (home com 12 partials) |
| `/cidade`, `/cultura`, `/grupos-populares`, `/gastronomia`, `/lugares` | `PaginasController` |
| `/lugares/{id}/{slug}`, `/grupos-populares/{id}/{slug}`, `/gastronomia/{id}/{slug}`, `/cultura/{id}/{slug}` | detalhes |
| `/noticias`, `/noticias/{slug}` e `/roteiros`, `/roteiros/{slug}` | Razor Pages (`Pages/`) |
| `/arquivo/{id}` | download de mídia |
| `/evento/{id}.ics` | agenda → calendário |
| `/api/analytics/event` | beacon de cliques |

**Partials da home** (`Views/Home/`): `_Hero` (navbar, preloader, hero),
`_Historia`, `_Cultura`, `_Gastronomia`, `_Maravilhas` (vitrine),
`_Agenda`, `_Mapa`, `_Roteiros`, `_Video`, `_Footer`, `Privacy`.

**Vitrine das 7 Maravilhas** — `Views/Shared/_VitrineMaravilhas.cshtml` (componente
reutilizado na home e em `/lugares`), JS em `portal.js` (`initWondersVitrine`):
postais com `role="tab"`/`aria-selected`, teclado ← →, swipe, crossfade da foto,
contador. **Manter o padrão `data-*`** ao estender.

---

## 10. SCSS (estilo)

- **`_portal.scss`** — CSS do protótipo portado **verbatim** (tokens `:root`,
  todas as classes originais). **Não editar regras aqui** (não reverter correções do
  protótipo); mudanças vão no `main.scss`.
- **`_tokens.scss`** — tokens reutilizáveis (`$shadow-card`, etc.).
- **`main.scss`** — `@use 'portal'` + **ajustes dinâmicos e overrides** (preloader,
  vitrine, páginas internas, responsividade mobile). Por vir **depois** no cascade, um
  override de mesma especificidade vence.
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
- **Segurança**: trocar a senha do admin, usar connection strings por secrets/env vars,
  HTTPS obrigatório (o `_Layout` gera canônicos/OG com `Request.Scheme`).

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

- **Itens de roteiro** editáveis no CMS (hoje vêm do seed — ver `RoteiroItem`).
- **Paginação** em Newsletter/Avaliações no painel.
- **Testes automatizados** (xUnit) para services e controllers — hoje a validação é
  build + smoke manual.
- **Redefinição de senha** por e-mail (`TurismoEstancia.Mail` está reservado para isso).
- **API/mobile** (`.Api` + `.MAUI`) seguindo o `PADRAO-DE-PROJETO.md`, se surgir demanda.
