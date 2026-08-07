# 📐 BLUEPRINT — Padrão de Arquitetura Genérico

> **O que é este arquivo:** documentação **genérica e reutilizável** do padrão de arquitetura do
> projeto **Prefeitura Digital** (ex-SIGESPE/Estância Digital). Serve de **template** para criar
> **novos projetos** com a mesma estrutura — ex.: um sistema de **Turismo** — seja fazendo
> você mesmo ou pedindo para o **Freebuff** gerar o esqueleto.
>
> **Como usar:**
> 1. Leia as seções para entender o padrão.
> 2. Ao criar um projeto novo, substitua os `{PLACEHOLDERS}` (ex.: `{NomeProjeto}`, `{Area}`).
> 3. Cole a seção **[Prompt para o Freebuff](#prompt-para-o-freebuff)** no chat e mande gerar o esqueleto.
> 4. Use a **[Checklist de criação](#checklist-de-criação-de-um-projeto-novo)** para não esquecer nenhum passo.

---

## 1. Visão geral do padrão

- **Plataforma:** ASP.NET Core **9** (MVC + Razor Pages), Entity Framework Core **9**, SQL Server.
- **Idioma:** código, comentários, views e textos de UI em **pt-BR** (`Solicitacao`, `Usuario`, `Noticia`...).
- **Encoding:** todos os arquivos em **UTF-8** (nunca ISO-8859-1).
- **Forma:** um projeto web "gordo" em **camadas** — o web project **não contém regra de negócio**;
  tudo vive em **class libraries** separadas, registradas por **injeção de dependência**.
- **Áreas MVC** separam **perfis de usuário** (cidadão, analista, admin...), cada uma com rota própria.
- **Banco:** um `DbContext` por domínio/bounded context; **seed desacoplado das migrações**.
- **Autorização:** policies baseadas em **claims** (não roles hard-coded no `[Authorize]`).

---

## 2. Estrutura de solução (o "esqueleto")

```
{NomeProjeto}.sln
├── {NomeProjeto}.Web/                     # Projeto de entrada (MVC + Razor Pages)
│   ├── Program.cs                         # Fino: só encadeia extensões
│   ├── Extensions/                        # Toda a composição vive aqui
│   │   ├── DatabaseExtensions.cs
│   │   ├── IdentityExtensions.cs
│   │   ├── BusinessServiceExtensions.cs
│   │   ├── InfrastructureExtensions.cs
│   │   └── PipelineExtensions.cs
│   ├── Areas/                             # UMA ÁREA POR PERFIL/USUÁRIO
│   │   └── {Area}/
│   │       ├── Controllers/
│   │       ├── Mappers/                   # Entidade/DTO → ViewModel
│   │       ├── Models/                    # ViewModels da área
│   │       ├── Views/
│   │       │   ├── Shared/                # _Layout{Area}.cshtml
│   │       │   ├── _ViewImports.cshtml
│   │       │   └── _ViewStart.cshtml
│   │       └── (Helpers/)                 # Helpers de view (ex.: badge de status)
│   ├── Controllers/                       # Controllers "fora de área" (portal público)
│   ├── Pages/                             # Razor Pages públicas (/Noticias, /Termos...)
│   ├── Services/                          # Infra do web project (background workers, queues)
│   └── wwwroot/                           # css/js/img/lib (SCSS compilado em build)
│
├── {NomeProjeto}.Domain/                  # (ex.: ModelsClassLibrary) ENTIDADES + DADOS
│   ├── Models/                            # Entidades POCO + DbContext + config OnModelCreating
│   ├── DTOs/                              # DTOs compartilhados
│   ├── Migrations/                        # Migrations do EF Core
│   ├── Seeding/                           # DatabaseSeeder (desacoplado das migrações!)
│   └── Validation/                        # Atributos de validação customizados
│
├── {NomeProjeto}.Services/                # (ex.: CommonServicesClassLibrary) REGRA DE NEGÓCIO
│   ├── {Modulo}/                          # 1 pasta por módulo de negócio
│   │   ├── Interfaces/INomeService.cs
│   │   └── Services/NomeService.cs
│   ├── Common/                            # Serviços transversais (versão, etc.)
│   └── (Factories/, Helpers/)             # Específicos do domínio
│
├── {NomeProjeto}.Authorization/           # (ex.: AuthorizationServicesClassLibrary)
│   └── Services/                          # ClaimObrigatoria + AppClaimHandler
│
├── {NomeProjeto}.Mail/                    # (ex.: MailServicesClassLibrary) E-MAIL (MailKit)
│   └── EmailSender.cs, FaleConosco.cs...
│
├── {NomeProjeto}.Identity/                # (ex.: IdentityAplicationLibrary) ASP.NET Identity
│   └── Models/Usuario.cs + IdentityContext (BANCO SEPARADO!)
│
└── (opcional)
    ├── {NomeProjeto}.Api/                 # API REST + JWT para app mobile
    └── {NomeProjeto}.Mobile/              # .NET MAUI (app mobile)
```

### Mapa de dependências (quem referencia quem)

```
{NomeProjeto}.Web ──► {NomeProjeto}.Services ──► {NomeProjeto}.Domain
        │  └────────► {NomeProjeto}.Domain
        ├──────────► {NomeProjeto}.Authorization
        ├──────────► {NomeProjeto}.Mail
        └──────────► {NomeProjeto}.Identity
```

> **Regra de ouro:** o fluxo de dependência aponta **sempre para baixo**. O web project referencia
> as libraries; o Domain (entidades) não referencia nada além do EF Core; Services referenciam
> Domain; Mail/Authorization/Identity são folhas.

### Referência concreta (projeto original → nome genérico)

| Projeto original            | Nome genérico            | Responsabilidade                        |
| --------------------------- | ------------------------ | --------------------------------------- |
| `PrefeituraDigital.Web`     | `{NomeProjeto}.Web`      | Apresentação, DI, rotas, areas, pipeline |
| `ModelsClassLibrary`        | `{NomeProjeto}.Domain`   | Entidades, DbContext, migrações, seed    |
| `CommonServicesClassLibrary`| `{NomeProjeto}.Services` | Regras de negócio por módulo            |
| `AuthorizationServicesClassLibrary` | `{NomeProjeto}.Authorization` | Claims e policies de autorização |
| `MailServicesClassLibrary`  | `{NomeProjeto}.Mail`     | Envio de e-mail (MailKit/SMTP)           |
| `IdentityAplicationLibrary` | `{NomeProjeto}.Identity` | ASP.NET Identity (banco próprio)        |
| `PrefeituraDigital.Api`     | `{NomeProjeto}.Api`      | (opcional) API JWT p/ app mobile         |
| `PrefeituraDigital.Mobile`  | `{NomeProjeto}.Mobile`   | (opcional) app .NET MAUI                 |

---

## 3. Ponto de entrada: Program.cs fino + Extensions

**`Program.cs` é propositalmente fino** — só encadeia os métodos de extensão:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddDatabase();
builder.AddIdentityConfig();
builder.AddBusinessServices();
builder.AddInfrastructure();

var app = builder.Build();

app.UseStandardPipeline();
app.MapAllRoutes();

await app.RunWithSeedSupportAsync(args);
```

Toda a composição vive em **`Extensions/`**, um arquivo por responsabilidade:

| Extensão | O que registra |
| --- | --- |
| `AddDatabase` | Todos os `DbContext` (um por connection string) |
| `AddIdentityConfig` | Identity, cookies, login social (opcional), **policies**, SMTP |
| `AddBusinessServices` | **Todos os serviços de negócio**, agrupados por módulo + workers |
| `AddInfrastructure` | Data Protection, upload limit, session/TempData, paginação, reCAPTCHA, MVC+Razor |
| `UseStandardPipeline` | Middleware (cultura pt-BR, static files, auth...) |
| `MapAllRoutes` | Rotas por área + rota padrão + Razor Pages |
| `RunWithSeedSupportAsync` | Suporte ao argumento `--seed` (bloqueado em produção) |

> **Convenção:** `BusinessServiceExtensions` e `MapAllRoutes` mantêm um comentário
> `// Novos módulos serão adicionados aqui` — é lá que você "plug-in" cada módulo novo.

---

## 4. Áreas MVC: uma área por perfil

Cada perfil de usuário do sistema vira uma **Área MVC**, com rota própria:

```csharp
// PipelineExtensions.cs → MapAllRoutes()
app.MapAreaControllerRoute(
    name: "Gestor",
    areaName: "Gestor",
    pattern: "Gestor/{controller=Dashboard}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "Solicitante",
    areaName: "Solicitante",
    pattern: "Solicitante/{controller=Solicitacao}/{action=Index}/{id?}");
```

**Exemplos no projeto original:**

| Área | Perfil | Uso |
| --- | --- | --- |
| `Solicitante` | Cidadão | Submeter e acompanhar solicitações |
| `Gestor` | Analista/gestor | Análise de solicitações, configuração de serviços |
| `Gerenciador` | Administrador (DTI) | Estrutura organizacional, usuários, status |
| `Identity` | — | Telas scaffolded do ASP.NET Identity (login/registro) |
| `Servidor` | Servidor público | Portal interno |

> **Regras da área:** cada área tem `Controllers/`, `Mappers/`, `Models/` (ViewModels) e `Views/`
> com `_Layout{Area}.cshtml` próprio. O roteamento da área é registrado em `MapAllRoutes`.

---

## 5. O padrão de MÓDULO de negócio (o mais importante)

Todo módulo novo (ex.: **Turismo**) segue o mesmo roteiro de 8 passos. É um padrão
**camada por camada**, do banco até a tela:

### Passo 1 — Entidade (no Domain)

```csharp
// {NomeProjeto}.Domain/Models/PontoTuristico.cs
public class PontoTuristico
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public string? Descricao { get; set; }
    public long? ImagemArquivoId { get; set; }   // aponta para a tabela Arquivo
    public bool Ativo { get; set; } = true;
    public int Ordem { get; set; }
}
```

### Passo 2 — DbSet + configuração (no `AppDbContext`)

```csharp
// DbContext.cs — DbSet
public DbSet<PontoTuristico> PontosTuristicos => Set<PontoTuristico>();

// DbContext.cs — OnModelCreating (configuração fluente da entidade)
modelBuilder.Entity<PontoTuristico>(entity =>
{
    entity.Property(e => e.Ativo).HasDefaultValue(true);
    entity.Property(e => e.Nome).HasMaxLength(200);
    entity.HasOne(e => e.Imagem)
          .WithMany()
          .HasForeignKey(e => e.ImagemArquivoId)
          .OnDelete(DeleteBehavior.SetNull);
});
```

> **Convenções de configuração:** `GetDATE()` via `HasDefaultValueSql` para datas,
> `HasDefaultValue(true)` para bools, `Restrict`/`SetNull` para FKs (evitar `Cascade` em
> relacionamentos de negócio), `HasConversion<string>()` para enums persistidos como string,
> índices únicos filtrados (`HasFilter`) para unicidade condicional.

### Passo 3 — DTO (no Domain, pasta `DTOs/`)

```csharp
public class PontoTuristicoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public string? Descricao { get; set; }
    public long? ImagemArquivoId { get; set; }
}
```

### Passo 4 — Interface + Service (no Services)

```csharp
// {NomeProjeto}.Services/Turismo/Interfaces/IPontoTuristicoService.cs
public interface IPontoTuristicoService
{
    Task<IReadOnlyList<PontoTuristicoDto>> ListarAsync(CancellationToken ct = default);
    Task<PontoTuristicoDto?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task CriarAsync(PontoTuristicoDto dto, IFormFile? imagem, CancellationToken ct = default);
    Task AtualizarAsync(PontoTuristicoDto dto, IFormFile? imagem, CancellationToken ct = default);
    Task RemoverAsync(int id, CancellationToken ct = default);
}

// {NomeProjeto}.Services/Turismo/Services/PontoTuristicoService.cs
public class PontoTuristicoService : IPontoTuristicoService
{
    private readonly AppDbContext _db;
    public PontoTuristicoService(AppDbContext db) => _db = db;

    // Projeção estática: Expression reutilizável Entity → DTO
    private static readonly Expression<Func<PontoTuristico, PontoTuristicoDto>> ToDto =
        p => new PontoTuristicoDto { Id = p.Id, Nome = p.Nome, Descricao = p.Descricao, ImagemArquivoId = p.ImagemArquivoId };

    public async Task<IReadOnlyList<PontoTuristicoDto>> ListarAsync(CancellationToken ct = default) =>
        await _db.PontosTuristicos.AsNoTracking()
            .Where(p => p.Ativo)
            .OrderBy(p => p.Ordem)
            .Select(ToDto)
            .ToListAsync(ct);
    // ...
}
```

> **Padrões do service:** injeta o `AppDbContext` direto; `AsNoTracking()` para leitura;
> projeção para DTO com `Expression` estática; métodos assíncronos com `CancellationToken`;
> `throw new InvalidOperationException("...não encontrado.")` para registros ausentes;
> uploads de imagem seguem o **padrão Arquivo** (byte[] na tabela `Arquivo`, servida por endpoint).

### Passo 5 — Registro na DI (`BusinessServiceExtensions`)

```csharp
builder.Services.AddScoped<IPontoTuristicoService, PontoTuristicoService>();
```

> **Sempre `AddScoped`** para serviços de negócio. Registre **todos** no
> `BusinessServiceExtensions`, agrupado por módulo, com comentário.

### Passo 6 — Controller na área + ViewModels/Mappers

```csharp
// Areas/Gerenciador/Controllers/PontoTuristicoController.cs
[Authorize(Policy = "Gerenciador")]
public class PontoTuristicoController : Controller
{
    private readonly IPontoTuristicoService _service;
    public PontoTuristicoController(IPontoTuristicoService service) => _service = service;

    public async Task<IActionResult> Index() =>
        View(await _service.ListarAsync());
    // ...
}
```

> O controller **não tem regra de negócio** — só traduz HTTP → service → ViewModel via Mapper.

### Passo 7 — Views (`.cshtml`)

- `Index.cshtml` — listagem (tabela/cards + paginação)
- `Formulario.cshtml` — formulário criar/editar
- Botões de ação secundária: `btn btn-sm`; botões primários de submit: `btn`.
  Ícones: 15px em `btn-sm`, 18px em `btn`.

### Passo 8 — (opcional) Seed

Veja a seção [7. Banco de dados](#7-banco-de-dados) — crie um `Seed{Modulo}Async` idempotente.

---

## 6. Upload de arquivos: o padrão `Arquivo`

Arquivos (imagens, PDFs) são persistidos em **byte[]** na tabela `Arquivo`, nunca no disco:

```csharp
// Service — criar/atualizar a imagem e retornar o Id
private async Task<long?> SalvarImagemAsync(IFormFile? imagem, long? arquivoExistenteId, CancellationToken ct)
{
    if (imagem is null || imagem.Length == 0) return arquivoExistenteId;

    using var ms = new MemoryStream();
    await imagem.CopyToAsync(ms, ct);

    if (arquivoExistenteId.HasValue) { /* atualiza bytes do existente e retorna o Id */ }

    var novo = new Arquivo { ArquFileName = ..., ArquBytes = ms.ToArray(), ... };
    _db.Arquivos.Add(novo);
    await _db.SaveChangesAsync(ct);
    return novo.ArquId;
}
```

- A entidade guarda `long? ImagemArquivoId` e o relacionamento com `SetNull`.
- A imagem é servida por um endpoint de download (`/portal/arquivo/{id}`) que lê os bytes e
  devolve com `Content-Type`/`FileDownloadName` corretos.

---

## 7. Banco de dados

> ⛔ **REGRAS PERMANENTES — PROIBIDO (vale para todo projeto que use este blueprint):**
>
> 1. **NUNCA rodar o seed** (argumento `--seed` / `DatabaseSeeder`) — em nenhum
>    ambiente, nem em desenvolvimento, nem para testes.
> 2. **NUNCA alterar o banco do Identity** (`{NomeProjeto}IdentityDb`): sem migrações,
>    sem SQL manual (`INSERT`/`UPDATE`/`DELETE`), sem criar/alterar/excluir usuários
>    ou claims — nem para validar funcionalidades. Contas são criadas **apenas** pela
>    tela de Usuários do painel, por quem tem acesso.
>
> Violar qualquer uma destas regras é considerado falha crítica.

### DbContexts

- **Um `DbContext` por bounded context**, registrado em `AddDatabase`:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(appCs, o => o.UseCompatibilityLevel(120)));
```

- **Identity em banco separado** (`IdentityContext`) — mantenha-o intocado; não adicione
  migrations/seed a ele.
- `Program.cs` é o startup project dos comandos EF; as migrations vivem na pasta `Migrations`
  da library do Domain.

### Migrations (comandos)

```bash
# O startup project é SEMPRE o web project ({NomeProjeto}.Web)
dotnet ef migrations add <Nome> --project {NomeProjeto}.Domain --startup-project {NomeProjeto}.Web --context AppDbContext
dotnet ef database update            --project {NomeProjeto}.Domain --startup-project {NomeProjeto}.Web --context AppDbContext
```

### Seed: desacoplado das migrações

**Regra sagrada do padrão:** NUNCA usar `HasData` nas entidades (senão o `database update`
semearia dados em produção). O seed é um **`DatabaseSeeder`** separado, executado só em
Development via `dotnet run --project {NomeProjeto}.Web -- --seed`:

```csharp
// {NomeProjeto}.Domain/Seeding/DatabaseSeeder.cs
public async Task SeedAsync(CancellationToken ct = default)
{
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
    if (string.Equals(env, "Production", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Seed BLOQUEADO em produção.");

    await using var tx = await _context.Database.BeginTransactionAsync(ct);
    try
    {
        await ResetSeedDataAsync(ct);      // limpa dados de seed na ordem correta de FK
        await SeedStatusAsync(ct);         // cada SeedXxx é idempotente:
        await SeedPrefeituraAsync(ct);     //   if (await _context.X.AnyAsync(ct)) return;
        await tx.CommitAsync(ct);
    }
    catch { await tx.RollbackAsync(ct); throw; }
}
```

- **Idempotência:** cada `SeedXxxAsync` começa com `if (await _context.Entidades.AnyAsync(ct)) return;`.
- Executa via `ExecuteSqlRawAsync` com `SET IDENTITY_INSERT` quando precisa de Ids fixos.
- `RunWithSeedSupportAsync` (no `PipelineExtensions`) intercepta o argumento `--seed`,
  **bloqueado se `ASPNETCORE_ENVIRONMENT != Development`** (dupla proteção: no Program.cs e no Seeder).
- Dados de status/fluxo (ex.: status de solicitação) são seed, **não schema** — ao evoluir em
  banco já migrado, insira via SQL manual.

> ⛔ **Lembrete:** a existência do mecanismo acima **não** autoriza executá-lo —
> rodar `--seed`/`DatabaseSeeder` é **proibido** (ver regras no topo da seção 7).

---

## 8. Autenticação e autorização

### Identity

- `AddIdentity<Usuario, IdentityRole>` com `IdentityContext` (banco próprio) + `AddDefaultUI()`
  (telas scaffolded na área `Identity`).
- Options: senha forte, lockout de 5 tentativas/5 min, `RequireConfirmedEmail`.
- Cookie: `.NomeProjeto.Auth`, `HttpOnly`, `SameSite=Lax`, `Secure` em produção, 8h com sliding.
- Login social (Google/Facebook): **opcional** — só registrado se as chaves existirem na config
  (ausência de chave esconde os botões automaticamente).

### Policies por claim (não roles hard-coded)

```csharp
// AuthorizationServicesClassLibrary/Services/
builder.Services.AddSingleton<IAuthorizationHandler, AppClaimHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Gestor", policy =>
        policy.AddRequirements(new ClaimObrigatoria("Gestor")));
    options.AddPolicy("Solicitante", policy =>
        policy.AddRequirements(new ClaimObrigatoria("Solicitante")));
    // ...
});

// Uso no controller:
[Authorize(Policy = "Gestor")]
```

---

## 9. Infraestrutura transversal

| Item | Configuração |
| --- | --- |
| **Cultura** | `pt-BR` forçada no pipeline (vírgula como decimal no model binding) |
| **Data Protection** | Chaves persistidas no SQL (`PersistKeysToDbContext`) com `SetApplicationName` |
| **Upload** | Limite de ~60 MB no Kestrel e IIS |
| **Session/TempData** | Cookies próprios, `HttpOnly`, `IsEssential`, `SameSite=Lax`, Secure em prod |
| **Cookie Policy** | `CheckConsentNeeded = false` (cookies essenciais sempre enviados) |
| **Paginação** | `ReflectionIT.Mvc.Paging` (view Bootstrap4, `pageindex`/`sort`) |
| **reCAPTCHA v3** | `GoogleRecaptchaV3Config` (seção na config) |
| **HttpContextAccessor** | Registrado p/ montar URLs absolutas fora de controllers |
| **SCSS** | `wwwroot/scss` → `wwwroot/css` compilado no build (`AspNetCore.SassCompiler`) |

---

## 10. Trabalhos em background, notificações e e-mail

**Padrão fila:** trabalhos lentos (e-mail, push, notificações) **nunca são aguardados** no
controller. Eles são enfileirados num `Channel` e processados por um `BackgroundService`:

```csharp
// Web/Services/NotificacaoBackgroundQueue.cs
//   Channel<Func<IServiceProvider, CancellationToken, Task>> (Singleton)

// Web/Services/NotificacaoBackgroundWorker.cs  (HostedService)
//   Consome a fila e executa cada job num scope

// No service chamador:
_notificacaoQueue.Enqueue(async (sp, ct) => await sp.GetRequiredService<IXService>().FazerAsync(ct));
```

- **E-mail:** `MailKit` via `EmailSender` (`Mail` library) + `IEmailQueue` (Channel) +
  `EmailBackgroundService` (worker). E-mails de notificação ao cidadão usam template HTML
  com detalhes (protocolo, status anterior → novo, link de acompanhamento).
- **Notificações in-app:** INSERT síncrono na tabela `Notificacao` (rápido), com regras por
  perfil (servidor = só in-app; cidadão = e-mail + in-app).
- **Push (mobile):** FCM via fila própria (`PushQueue` + `PushBackgroundService`); em
  ambiente sem FCM usa-se `NoOpPushNotificationSender`.
- **Workers de prazo:** `PrazoTipoSolicitacaoWorker` (hosted service) roda periodicamente
  para inativar/expirar itens (ex.: formulários com antecedência mínima).

> Todos os `HostedService` são registrados em `BusinessServiceExtensions`.

---

## 11. Convenções gerais (checklist de estilo)

- [ ] Identificadores, comentários e textos de UI em **pt-BR**.
- [ ] Arquivos em **UTF-8**.
- [ ] `Program.cs` fino; tudo em `Extensions/`.
- [ ] Regra de negócio **só** nos services (nunca no controller).
- [ ] 1 módulo = 1 pasta em Services com `Interfaces/` + `Services/`.
- [ ] Service registrado com `AddScoped` no `BusinessServiceExtensions`.
- [ ] Área nova registrada em `MapAllRoutes`.
- [ ] Leitura com `AsNoTracking()` + projeção `ToDto`.
- [ ] Datas com `HasDefaultValueSql("GETDATE()")`; bools com `HasDefaultValue(true)`.
- [ ] Seed idempotente em `DatabaseSeeder`, nunca `HasData`.
- [ ] Claims/policies para autorização (não roles literais).
- [ ] Trabalhos lentos enfileirados em `Channel` + `BackgroundService`.
- [ ] Botões: `btn` (primário) / `btn btn-sm` (ações em cards/tabelas) — não misturar no mesmo grupo.
- [ ] Upload de arquivos via tabela `Arquivo` (byte[]), nunca no disco.

---

## 12. Checklist de criação de um projeto novo

> Exemplo prático: **sistema de Turismo** (`{NomeProjeto} = TurismoDigital`).

**A. Esqueleto da solução**
- [ ] Criar a solution e os projetos (Web, Domain, Services, Authorization, Mail, Identity) com
      as dependências do [Mapa de dependências](#mapa-de-dependências-quem-referencia-quem).
- [ ] `Program.cs` fino + 5 `Extensions/` com o conteúdo mínimo.
- [ ] Connection strings em `ConnectionStrings:*` (flat keys), secrets em User Secrets / env vars
      (nunca commitados).
- [ ] Cultura pt-BR + cookies + upload limit + Data Protection.

**B. Dados**
- [ ] `AppDbContext` com os DbSets do domínio novo + config fluente no `OnModelCreating`.
- [ ] `dotnet ef migrations add Inicial` e `database update`.
- [ ] `DatabaseSeeder` com guard de produção + `SeedAsync` idempotente.
- [ ] Seed dos dados "de referência" do domínio (ex.: status, categorias, pontos turísticos iniciais).

**C. Módulos de negócio (repetir para cada módulo: PontoTuristico, Roteiro, Reserva...)**
- [ ] Entidade → DbSet → config → DTO → `INomeService` → `NomeService` → registro DI.
- [ ] Controller na área certa com `[Authorize(Policy = "...")]`.
- [ ] Views (Index + Formulario) + Mapper/ViewModel.
- [ ] Rota da área em `MapAllRoutes` (se área nova).
- [ ] `dotnet build` limpo + teste manual do CRUD.

**D. (Opcional) Identidade/perfis**
- [ ] Roles/claims por perfil (ex.: `Gestor`, `Operador`, `Visitante`) + policies.
- [ ] Login social só se houver chaves.

**E. (Opcional) Notificações/e-mail**
- [ ] `IEmailQueue` + `EmailBackgroundService`; template HTML do e-mail.
- [ ] Fila de notificações in-app + sino no layout.

---

## 13. Prompt para o Freebuff

> Cole o bloco abaixo (junto com este arquivo) para o Freebuff gerar o esqueleto de um projeto
> novo seguindo este padrão. Ajuste os `{...}` conforme seu domínio.

````text
Crie um novo projeto ASP.NET Core 9 seguindo EXATAMENTE o padrão documentado no arquivo
PADRAO-DE-PROJETO.md deste repositório.

Projeto: {NomeProjeto} — sistema de {domínio, ex.: turismo municipal}.

Requisitos:
1. Solução com os projetos: {NomeProjeto}.Web, {NomeProjeto}.Domain, {NomeProjeto}.Services,
   {NomeProjeto}.Authorization, {NomeProjeto}.Mail, {NomeProjeto}.Identity, com as
   dependências corretas (Web → Services → Domain; folhas: Authorization, Mail, Identity).
2. Program.cs fino + Extensions/ (Database, Identity, BusinessServices, Infrastructure,
   Pipeline) com placeholder `// Novos módulos serão adicionados aqui`.
3. Entidades do domínio: {listar entidades, ex.: PontoTuristico, Roteiro, Reserva, Avaliacao}
   com DbSet + configuração fluente no AppDbContext (datas GETDATE(), bools default true,
   FKs Restrict/SetNull, enums como string).
4. Áreas MVC por perfil: {perfis, ex.: Visitante (público), Gestor (admin)} com rota em
   MapAllRoutes e layouts próprios.
5. Para cada módulo: DTO → Interface + Service (Interfaces/ + Services/) → registro
   AddScoped em BusinessServiceExtensions → Controller com [Authorize(Policy=...)] → Views
   (Index + Formulario) com botões `btn`/`btn btn-sm`.
6. Upload de imagens via tabela Arquivo (byte[]) + endpoint de download.
7. DatabaseSeeder idempotente desacoplado das migrações (nunca HasData), com guard de
   produção e suporte a `--seed`.
8. Policies de autorização por claim em {NomeProjeto}.Authorization.
9. pt-BR em todo código/UI; arquivos UTF-8.
10. Comandos de migração documentados no README do novo projeto.
````

---

*Documento gerado a partir da análise do projeto Prefeitura Digital (SIGESPE). Mantenha este
arquivo na raiz do repositório e copie-o para novos projetos como referência de arquitetura.*
