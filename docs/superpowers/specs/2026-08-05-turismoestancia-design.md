# Design — TurismoEstância (Portal de Turismo de Estância/SE)

> **Data:** 2026-08-05 · **Status:** aprovado em brainstorming · **Próximo passo:** plano de implementação (writing-plans)

## 1. Contexto

O protótipo estático `Prototipo/index.html` ("Descubra Estância — Capital Sergipana da Cultura") será transformado em um **sistema web dinâmico** em ASP.NET Core 9, seguindo **exatamente** o padrão de arquitetura documentado em `PADRAO-DE-PROJETO.md`.

Tudo o que hoje está fixo no protótipo (textos, imagens, slides, eventos, pontos turísticos, mapa, estatísticas, footer) passa a ser **conteúdo gerenciável** por um painel administrativo.

## 2. Decisões de produto (acordadas)

1. **Escopo v1 completa:** todas as seções do portal são dinâmicas (Maravilhas, Agenda, Mapa, Gastronomia, Cultura, Estatísticas, Newsletter, e extensões).
2. **Dois perfis de acesso:** `Gerenciador` (acesso total ao CMS) e `Operador` (apenas **Evento** e **InscricaoNewsletter**).
3. **Eventos reais:** `DataInicio`/`DataFim`; o portal mostra apenas eventos **futuros**; "Adicionar à Agenda" **gera arquivo `.ics`**.
4. **Guia do turista:** o CTA "Baixe o Guia" **só aparece quando houver** arquivo de guia cadastrado no CMS.
5. **Newsletter real:** e-mail salvo no banco com **consentimento LGPD**; Gerenciador/Operador lista, exporta **CSV** e inativa.
6. **Extensões no v1:** `Noticia`, `Avaliacao` (com moderação), `Roteiro` (+ itens) e `HorarioFuncionamento`.

## 3. Arquitetura

- **Nome da solução:** `TurismoEstancia` (o repositório já se chama TurismoEstancia).
- **Stack:** ASP.NET Core 9 (MVC + Razor Pages), EF Core 9, SQL Server, pt-BR, UTF-8.
- **Abordagem escolhida (A):** MVC fiel ao padrão; o Razor renderiza o mesmo HTML do protótipo com dados vindos dos services. O array `allPois` do mapa é servido como JSON serializado na view. O CSS do protótipo é portado para SCSS.

### 3.1 Solução (mapa de dependências do padrão)

```
TurismoEstancia.sln
├── TurismoEstancia.Web/            # MVC + Razor + SCSS; Program.cs fino + Extensions/
├── TurismoEstancia.Domain/         # Entidades, AppDbContext, Migrations, DatabaseSeeder (+ Media/)
├── TurismoEstancia.Services/       # 1 pasta por módulo: Interfaces/ + Services/
├── TurismoEstancia.Authorization/  # ClaimObrigatoria + AppClaimHandler + policies
├── TurismoEstancia.Identity/       # Usuario + IdentityContext (banco separado)
└── TurismoEstancia.Mail/           # Stub no v1 (sem fluxo de e-mail) — projeto entra na solução
```

- **Bancos:** `TurismoEstanciaDb` (app) e `TurismoEstanciaIdentityDb` (Identity).
- **Program.cs fino** + `Extensions/` (`DatabaseExtensions`, `IdentityExtensions`, `BusinessServiceExtensions`, `InfrastructureExtensions`, `PipelineExtensions`) com marcador `// Novos módulos serão adicionados aqui`.

### 3.2 Visual

- `wwwroot/scss/`: `_tokens.scss` (design tokens do protótipo), `_mixins.scss`, `_base.scss` → `main.scss` (portal, **classes idênticas ao protótipo**) e `painel.scss` (CMS com os mesmos tokens).
- Compilação em build via `AspNetCore.SassCompiler`.

## 4. Modelo de dados (18 entidades em `TurismoEstancia.Domain`)

Convenções do padrão: ids `int` (`Arquivo.Id` = `long`), datas com `HasDefaultValueSql("GETDATE()")`, bools com `HasDefaultValue(true)`, enums persistidos como **string**, FKs `Restrict`/`SetNull` (filhos próprios = `Cascade`), nomes pt-BR.

### 4.1 Turismo

| Entidade | Campos | Regras |
|---|---|---|
| `CategoriaPontoTuristico` | Nome, SubTitulo, Cor (hex), Icone (lucide), ApresentarEmMaravilhas, ExibirNoMapa, Ordem, Ativo | alimenta a seção "7 Maravilhas" e o mapa |
| `PontoTuristico` | Nome, Descricao, Detalhe, Tag, Icone, CategoriaId→FK, Endereco, ComoChegar, LeftPercent (`int` 0–100), TopPercent (`int` 0–100), ExibirNoMapa, Ordem, Ativo | `Left/TopPercent` = posição no mapa ilustrado |
| `PontoTuristicoMidia` | PontoTuristicoId→FK, ArquivoId→FK, Tipo (Capa\|Pictograma\|Galeria), Ordem | índice único filtrado: **1 Capa** por ponto |
| `HorarioFuncionamento` | PontoTuristicoId→FK, DiaSemana (enum: Domingo…Sabado), HoraInicio (`TimeOnly`), HoraFim, Fechado | validação: se aberto, Fim > Inicio |
| `Evento` | Titulo, Descricao, Local, DataInicio, DataFim, Ordem, Ativo | portal mostra só futuros; validação Fim ≥ Inicio |
| `Slide` | ImagemArquivoId→FK, Ordem, Ativo | slides do hero |
| `Estatistica` | Valor (string), Legenda, Ordem, Ativo | "192 · anos" |

### 4.2 Cultura & Gastronomia

| Entidade | Campos |
|---|---|
| `GrupoCultural` | Nome, Ordem, Ativo |
| `PratoTuristico` | Nome, Ordem, Ativo |
| `TagCultural` | Nome (com emoji), Ordem, Ativo |

### 4.3 Conteúdo & Configuração

| Entidade | Campos | Nota |
|---|---|---|
| `ConteudoSite` | Chave (**única**), Nome, Texto | blocos de texto das seções (~25 chaves) |
| `ConfiguracaoSite` | Chave (**única**), Nome, Tipo (Texto\|Arquivo), ValorTexto, ArquivoId→FK | guia PDF, vídeo institucional, título/meta SEO |
| `Contato` | Tipo (Endereco\|Telefone\|RedesSocial), Rotulo, Valor, Icone, Ordem, Ativo | footer: 190, SAMU, SMTT, CIT, redes sociais |

### 4.4 Comunicação

| Entidade | Campos | Nota |
|---|---|---|
| `InscricaoNewsletter` | Email (**única**), Origem, ConsentimentoLgpd, DataInscricao, Ativo | reenvio reativa; exclusão = `Ativo=false` |
| `Noticia` | Titulo, Resumo, Corpo, ImagemArquivoId→FK, DataPublicacao, Slug (**única**), Publicada, Ordem, Ativo | portal `/Noticias` + CMS |

### 4.5 Avaliação & Roteiros

| Entidade | Campos | Nota |
|---|---|---|
| `Avaliacao` | PontoTuristicoId→FK, Nome, Nota (1–5), Comentario, Data, Aprovada (default false) | moderação no CMS |
| `Roteiro` | Titulo, Descricao, ImagemArquivoId→FK, Ordem, Ativo | — |
| `RoteiroItem` | RoteiroId→FK, PontoTuristicoId→FK, Dia, Ordem, Observacao | validação Dia ≥ 1 |

### 4.6 Infra

| Entidade | Campos |
|---|---|
| `Arquivo` | Id (**long**), Nome, ContentType, Bytes, CriadoEm |

**FKs:** filhos próprios (`PontoTuristicoMidia`, `HorarioFuncionamento`, `RoteiroItem`, `Avaliacao`) = **Cascade**; referências a entidades compartilhadas (`Categoria`, `Arquivo`) = **Restrict/SetNull**.

**Índices únicos:** Chave de `ConteudoSite` e `ConfiguracaoSite`, Email de `InscricaoNewsletter`, Slug de `Noticia`, e 1 Capa por `PontoTuristico` (único filtrado).

## 5. Áreas, autorização e páginas

### 5.1 Portal público (sem login)

- `HomeController.Index` → `HomeViewModel` + partials: `_Hero`, `_Video`, `_Historia`, `_Cultura`, `_Gastronomia`, `_Maravilhas`, `_Agenda`, `_Mapa`, `_Roteiros`, `_Footer`.
- POSTs: **Newsletter** (e-mail + checkbox LGPD) e **Avaliação** (nome, nota 1–5, comentário → "aguardando moderação").
- Avaliações **aprovadas** aparecem no modal do ponto/mapa.
- Endpoints: `GET /Evento/{id}/ics` (baixa `.ics`), `GET /arquivo/{id}` (serve mídias com Content-Type correto).
- Razor Pages públicas: `/Noticias` (lista) e `/Noticias/{slug}` (detalhe); `/Roteiros` e `/Roteiros/{id}` (itens + pontos).

### 5.2 Área `Gerenciador` (`/Gerenciador`, policy `Gerenciador`)

Dashboard; PontoTuristico (form com **Mídias + Horários** no mesmo formulário); Categoria; Evento; Slide; Estatistica; GrupoCultural; PratoTuristico; TagCultural; ConteudoSite; ConfiguracaoSite; Contato; Noticia; Roteiro; **Avaliacao** (moderação: aprovar/excluir); InscricaoNewsletter; **Usuario** (cria contas e atribui perfil).

### 5.3 Área `Operador` (`/Operador`, policy `Operador`)

Dashboard; **Evento** (CRUD); **InscricaoNewsletter** (lista + CSV + inativar). Sem acesso aos demais módulos.

### 5.4 Identity

- `Usuario : IdentityUser` com `NomeCompleto`; perfis via **claims** (`Gerenciador`, `Operador`) → policies `ClaimObrigatoria` + `AppClaimHandler` (nunca roles literais).
- Contas criadas pelo Gerenciador (sem auto-registro); `RequireConfirmedEmail = false` no v1; lockout 5 tentativas/5 min; senha forte; cookie `.TurismoEstancia.Auth` (HttpOnly, SameSite=Lax, Secure em prod).
- Seed (dev): usuário admin com claim `Gerenciador`.

## 6. Seed (`DatabaseSeeder`)

- Desacoplado das migrações (**nunca `HasData`**), bloqueado em produção (guarda dupla: Program.cs + Seeder).
- Idempotente por módulo: `SeedCategorias`, `SeedPontosTuristicos`, `SeedEventos`, `SeedSlides`, `SeedEstatisticas`, `SeedGrupos`, `SeedPratos`, `SeedTags`, `SeedConteudo`, `SeedContatos`, `SeedConfiguracoes`, `SeedNoticias`, `SeedRoteiros`, `SeedAvaliacoes` (1 aprovada + 1 pendente), `SeedUsuarioAdmin`.
- Mídias: binários copiados de `Prototipo/img` para `TurismoEstancia.Domain/Seeding/Media/`, gravados em byte[] na tabela `Arquivo` (Capas/Pictogramas, slides, vídeo). **Guia sem arquivo no seed** (demonstra CTA oculto).
- Execução: `dotnet run --project TurismoEstancia.Web -- --seed` (só Development).

## 7. Mídia, erros e validação

- Upload ~60MB; validação por tipo (jpeg/png → imagens; mp4 → vídeo; pdf → guia) e tamanho.
- Imagens órfãs apagadas pelo service ao trocar/remover.
- Services lançam `InvalidOperationException("... não encontrado.")` → 404 estilizado (páginas de 404/500 no visual do portal).
- Validações pt-BR via ModelState: `DataFim ≥ DataInicio`, `HoraFim > HoraInicio` (quando aberto), `Nota 1–5`, `Dia ≥ 1`, e-mail único.
- Feedback por TempData com retorno âncora (sem perder scroll).

## 8. Módulos → Services (`TurismoEstancia.Services`, 1 pasta por módulo)

| Módulo | Services | Registro |
|---|---|---|
| Turismo | CategoriaPontoTuristico, PontoTuristico (Midia+Horario no form), Evento, Slide, Estatistica | `AddScoped` |
| CulturaGastronomia | GrupoCultural, PratoTuristico, TagCultural | `AddScoped` |
| Conteudo | ConteudoSite, ConfiguracaoSite, Contato | `AddScoped` |
| Comunicacao | InscricaoNewsletter, Noticia | `AddScoped` |
| Roteiro | Roteiro | `AddScoped` |
| Avaliacao | Avaliacao | `AddScoped` |
| Infra | Arquivo | `AddScoped` |

Padrões por service: injeta `AppDbContext`; `AsNoTracking()` em leitura; projeção DTO com `Expression` estática; `CancellationToken`; `InvalidOperationException` para ausentes.

## 9. Testes / validação (checklist do padrão)

- `dotnet build` limpo após cada módulo; migração `Inicial` + `database update` em dev.
- Seed → conferir o portal seção a seção (hero, vídeo, história, cultura, gastronomia, 7 Maravilhas, agenda, mapa, modal com avaliações, roteiros, notícias, newsletter, footer).
- CRUDs do Gerenciador; confirmar que **Operador só acessa Evento e Newsletter**.
- Importar `.ics` no Google Calendar; exportar CSV; guia oculto sem arquivo → visível com arquivo.
- Sem projeto de testes unitários no v1 (o padrão não prevê — validação por build + teste manual).

## 10. Fora de escopo do v1

- Projetos `.Api` (JWT) e `.Mobile` (.NET MAUI) — o padrão já prevê; ficam para depois.
- E-mail transacional (a newsletter v1 é captura + CSV; o `Mail` é stub).
- Auditoria de ações do CMS (candidata a v1.5).
