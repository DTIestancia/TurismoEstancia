# 🌴 TurismoEstancia — Portal de Turismo de Estância/SE

Portal oficial de turismo de **Estância/SE — Capital Sergipana da Cultura**, com CMS para gestão
de conteúdo. Construído em **ASP.NET Core 9** seguindo o padrão de arquitetura do
[`PADRAO-DE-PROJETO.md`](./PADRAO-DE-PROJETO.md).

## ✨ O que o sistema faz

- **Portal público** (a antiga `Prototipo/index.html`, agora 100% dinâmica):
  hero com slides, seção de história com estatísticas, cultura & gastronomia,
  **7 Maravilhas** com cards por categoria, **mapa interativo** com os POIs
  (filtros, legenda, modal com avaliações), agenda de eventos com exportação
  **.ics**, roteiros, notícias e rodapé com newsletter (LGPD).
- **CMS** com dois perfis (policies por **claim**, nunca roles literais):
  - **Gerenciador** — acesso total: categorias, pontos turísticos (mídias +
    horários), eventos, slides, estatísticas, grupos culturais, pratos, tags,
    textos do portal, configurações (guia/vídeo/SEO), contatos, notícias,
    roteiros, moderação de avaliações, newsletter (CSV) e usuários.
  - **Operador** — restrito a **Eventos** e **Newsletter**.
- **Login próprio** (sem auto-registro público — contas criadas pelo Gerenciador).

## 🚀 Como rodar

Pré-requisitos: **.NET 9 SDK**, **SQL Server** (LocalDB ou instância local).

```bash
# 1. Restaurar e aplicar migrações + importar os dados do protótipo (seed)
dotnet run --project TurismoEstancia.Web -- --seed

# 2. Subir o portal
dotnet run --project TurismoEstancia.Web
```

- **Portal:** `http://localhost:5xxx` (ver porta no console)
- **Painel:** `/Identity/Account/Login` (o acesso às áreas `/Gerenciador` e `/Operador`
  redireciona para o login)
- **Usuário admin (Gerenciador) criado pelo seed:**
  - e-mail: `admin@estancia.se.gov.br`
  - senha: `Estancia@2026`

> ⚠️ Troque a senha em produção. O seed é idempotente: rodar de novo não duplica nada.
> A senha do admin também pode ser redefinida pelo Gerenciador no painel (Usuários → Excluir
> e recriar) — em versões futuras haverá redefinição de senha.

## 🗄️ Bancos

| Banco                     | Connection string (appsettings.json)     | Conteúdo                    |
| ------------------------- | ---------------------------------------- | --------------------------- |
| `TurismoEstanciaDb`       | `TurismoEstancia`                        | Domínio do portal (negócio) |
| `TurismoEstanciaIdentityDb` | `TurismoEstanciaIdentity`              | ASP.NET Identity (usuários) |

Mídias (imagens, vídeo, guia) ficam em **byte[] no banco**, servidas por `GET /arquivo/{id}`.
A tabela `Arquivos` segue o padrão **`PrefeituraDigital.Arquivo`** (colunas `ArquId`,
`ArquUID` ROWGUIDCOL, `ArquFileName`, `ArquContentType`, `ArquSize`, `ArquBytes`
`varbinary(max)`, `ArquMomento`, `ArquAutor`, `ArquAtivo`, `ArquOrigem`) e está **pronta
para FILESTREAM**: quando o filegroup for criado no servidor, basta executar
[`Deploy/01-Filestream-Config.sql`](./Deploy/01-Filestream-Config.sql) para converter o
`ArquBytes` em `varbinary(max) FILESTREAM` — sem nenhuma mudança de código (o EF já
lê/grava o binário da mesma forma).

## 🏗️ Arquitetura

```
TurismoEstancia.slnx
├── TurismoEstancia.Web/            # Entrada (MVC + Razor Pages + Areas)
│   ├── Areas/Gerenciador/          # CMS completo (16 CRUDs)
│   ├── Areas/Operador/             # Eventos + Newsletter
│   ├── Controllers/                # Portal público + endpoints (.ics, arquivo, newsletter, avaliação)
│   ├── Pages/                      # Notícias e Roteiros (Razor Pages)
│   ├── Views/Home/                 # 10 partials do portal
│   └── wwwroot/scss/               # SCSS portado do protótipo (compila p/ css no build)
├── TurismoEstancia.Domain/         # Entidades, enums, DTOs e AppDbContext (+ migrações)
├── TurismoEstancia.Services/       # 7 módulos de serviços (interface + implementação)
├── TurismoEstancia.Authorization/  # Policies por claim (Gerenciador/Operador)
├── TurismoEstancia.Identity/       # ASP.NET Identity (banco separado)
└── TurismoEstancia.Mail/           # (reservado) e-mail
```

### Convenções (PADRAO-DE-PROJETO)

- Código, views e mensagens em **pt-BR**; arquivos em **UTF-8**.
- `AsNoTracking` em leituras, projeção `ToDto`, `CancellationToken` em toda query.
- `InvalidOperationException` (mensagem pt-BR) para entidade ausente.
- Enums armazenados como **string**; datas com `GETDATE()`; bools com default.
- FKs `Restrict`/`SetNull` (Cascade apenas em filhos próprios).
- Seed **desacoplado** das migrações (`--seed`), nunca dentro de `OnModelCreating`.

## 🧪 Validação

- Build da solução: `dotnet build TurismoEstancia.slnx` (0 erros / 0 avisos).
- Smoke tests executados nas fases 5–7: portal 200, login 302→dashboard, CRUD
  criando registros no banco, restrição do Operador (admin → AccessDenied),
  exportação `.ics` e newsletter persistindo com consentimento LGPD.

## 📌 Roadmap v1.1 (sugestões)

- Redefinição de senha de usuários (e-mail) e edição de perfil.
- Edição dos **itens de roteiro** no CMS (hoje vêm do seed).
- Paginação nas listagens do painel (Newsletter/Avaliações).
- Upload do **guia em PDF real** (o seed usa uma imagem como placeholder).
