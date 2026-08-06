# Plano de Implementação — TurismoEstância

> **Data:** 2026-08-05 · **Origem:** `docs/superpowers/specs/2026-08-05-turismoestancia-design.md` (aprovado) + `PADRAO-DE-PROJETO.md`
> **Base:** branch `dev` · **Target:** `net9.0` (runtime 9.0.18 presente; SDK 10 compila)

## Fase 0 — Ambiente

- [ ] 0.1 Confirmar branch `dev` ativa (trabalho nunca em `main` sem consentimento).
- [ ] 0.2 Confirmar `dotnet` (SDK 10 + runtime 9), `dotnet ef` e SQL Server (sqlcmd) disponíveis.

## Fase 1 — Esqueleto da solução

- [ ] 1.1 `dotnet new sln TurismoEstancia` + `dotnet new classlib/mvc` para os 6 projetos: `TurismoEstancia.Web` (mvc), `.Domain`, `.Services`, `.Authorization`, `.Mail`, `.Identity` (classlibs).
- [ ] 1.2 Referências conforme mapa de dependências: Web→Services/Domain/Authorization/Mail/Identity; Services→Domain; demais folhas.
- [ ] 1.3 Pacotes: EF Core SqlServer (Domain), Identity EF (Identity), `AspNetCore.SassCompiler` + `ReflectionIT.Mvc.Paging` (Web), MailKit (Mail).
- [ ] 1.4 `Program.cs` fino + `Extensions/`: Database, Identity, BusinessServices, Infrastructure, Pipeline — com marcadores `// Novos módulos serão adicionados aqui`.
- [ ] 1.5 `appsettings.json` (ConnectionStrings flat keys, cultura pt-BR, upload ~60MB, Data Protection) + User Secrets (sem segredos commitados).
- [ ] 1.6 SCSS: `_tokens.scss` (tokens do protótipo), `_mixins.scss`, `_base.scss`, `main.scss` (portal), `painel.scss` (CMS).
- [ ] 1.7 **Verificação:** `dotnet build` limpo; app sobe.

## Fase 2 — Domain (entidades + bancos)

- [ ] 2.1 18 entidades em `Models/` (spec §4): CategoriaPontoTuristico, PontoTuristico, PontoTuristicoMidia, HorarioFuncionamento, Evento, Slide, Estatistica, GrupoCultural, PratoTuristico, TagCultural, ConteudoSite, ConfiguracaoSite, Contato, InscricaoNewsletter, Noticia, Avaliacao, Roteiro, RoteiroItem, Arquivo.
- [ ] 2.2 `AppDbContext`: DbSets + `OnModelCreating` (datas `GETDATE()`, bools default, enums string, FKs Restrict/SetNull + Cascade nos filhos próprios, índices únicos: Chaves, Email, Slug, 1 Capa filtrado).
- [ ] 2.3 Projeto Identity: `Usuario : IdentityUser` (+ NomeCompleto) + `IdentityContext` (banco separado, intocado).
- [ ] 2.4 DTOs por entidade (pasta `DTOs/`).
- [ ] 2.5 `dotnet ef migrations add Inicial` + `database update` (dev, startup project = Web).
- [ ] 2.6 **Verificação:** build + migrations aplicadas.

## Fase 3 — Authorization + Identity

- [ ] 3.1 `ClaimObrigatoria` + `AppClaimHandler` (projeto Authorization).
- [ ] 3.2 Policies `Gerenciador` e `Operador` (claims, não roles).
- [ ] 3.3 Identity config: cookie `.TurismoEstancia.Auth`, lockout 5/5min, senha forte, `RequireConfirmedEmail=false` (v1).
- [ ] 3.4 **Verificação:** build.

## Fase 4 — Services (7 módulos)

- [ ] 4.1 Turismo: CategoriaPontoTuristico, PontoTuristico (mídias + horários no mesmo form), Evento (só futuros + geração `.ics`), Slide, Estatistica.
- [ ] 4.2 CulturaGastronomia: GrupoCultural, PratoTuristico, TagCultural.
- [ ] 4.3 Conteudo: ConteudoSite, ConfiguracaoSite, Contato.
- [ ] 4.4 Comunicacao: InscricaoNewsletter (email único, ativar/inativar, export CSV), Noticia (slug único, Publicada).
- [ ] 4.5 Roteiro: Roteiro (com itens Dia/Ordem/Observacao).
- [ ] 4.6 Avaliacao: submissão (Aprovada=false) + moderação (aprovar/excluir).
- [ ] 4.7 Infra: Arquivo (gravar byte[], servir com Content-Type, limpar órfãos).
- [ ] 4.8 Registro `AddScoped` em `BusinessServiceExtensions`.
- [ ] 4.9 **Verificação:** build.

## Fase 5 — Portal público

- [ ] 5.1 `HomeController.Index` + `HomeViewModel` + 10 partials portando o HTML do protótipo (`_Hero`, `_Video`, `_Historia`, `_Cultura`, `_Gastronomia`, `_Maravilhas`, `_Agenda`, `_Mapa`, `_Roteiros`, `_Footer`) com dados do Razor.
- [ ] 5.2 JS adaptado em `wwwroot/js/`: carrossel, tema, partículas, reveal, mapa (`allPois` via `@Json.Serialize`), modal com **avaliações aprovadas** + form de avaliação.
- [ ] 5.3 Endpoints: `GET /arquivo/{id}`, `GET /Evento/{id}/ics`, POST newsletter (LGPD), POST avaliação.
- [ ] 5.4 Razor Pages: `/Noticias` (lista) + `/Noticias/{slug}`; `/Roteiros` + `/Roteiros/{id}`.
- [ ] 5.5 Páginas 404/500 no visual do portal.
- [ ] 5.6 **Verificação:** build + conferir seções com seed.

## Fase 6 — CMS (Áreas Gerenciador + Operador)

- [ ] 6.1 Layouts do painel + `painel.scss` + badges/`btn`/`btn-sm` + paginação.
- [ ] 6.2 Área Gerenciador: Dashboard e CRUDs — Categoria, PontoTuristico (mídias+horários), Evento, Slide, Estatistica, GrupoCultural, PratoTuristico, TagCultural, ConteudoSite, ConfiguracaoSite (guia/vídeo/SEO), Contato, Noticia, Roteiro, Avaliacao (moderação), InscricaoNewsletter, Usuario.
- [ ] 6.3 Área Operador: Dashboard, Evento (CRUD), InscricaoNewsletter (lista+CSV+inativar) — **sem acesso** aos demais.
- [ ] 6.4 Rotas `MapAreaControllerRoute` no `MapAllRoutes`.
- [ ] 6.5 **Verificação:** build + CRUDs manuais + restrição do Operador.

## Fase 7 — Seed (desacoplado das migrações)

- [ ] 7.1 Copiar binários de `Prototipo/img` → `TurismoEstancia.Domain/Seeding/Media/` (imagens, pictogramas, vídeo).
- [ ] 7.2 `DatabaseSeeder` + `SeedXxxAsync` idempotentes por módulo (spec §6) + guard de produção (nunca `HasData`).
- [ ] 7.3 `SeedUsuarioAdmin` (dev): usuário com claim Gerenciador.
- [ ] 7.4 **Verificação:** `dotnet run --project TurismoEstancia.Web -- --seed` roda em dev; bloqueado fora de Development.

## Fase 8 — Validação final & entrega

- [ ] 8.1 Checklist do spec: portal seção a seção; `.ics` importa no Google Calendar; CSV da newsletter; guia oculto sem arquivo → visível com arquivo; Operador só acessa Evento+Newsletter.
- [ ] 8.2 `dotnet build` limpo (Debug).
- [ ] 8.3 Commit na `dev`; decisão de merge/PR para `main` com o usuário.
- [ ] 8.4 README com comandos de migração, seed e credenciais de dev.
