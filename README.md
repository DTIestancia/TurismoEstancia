# 🌴 TurismoEstancia — Portal de Turismo de Estância/SE

Portal oficial de turismo de **Estância/SE — Capital Sergipana da Cultura**, com CMS para gestão
de conteúdo. Construído em **ASP.NET Core 9** seguindo o padrão de arquitetura do
[`PADRAO-DE-PROJETO.md`](./PADRAO-DE-PROJETO.md).

## ✨ O que o sistema faz

- **Portal público** (100% dinâmico):
  **galeria de fotos por categorias dinâmicas** (`/galeria`, com lightbox e
  thumbnails — as fotos são **otimizadas no upload**: redimensionadas para
  1600px, re-encodadas como JPEG e sem metadados EXIF, gerando também um
  thumbnail de 400px; uma foto de celular de 5 MB vira ~200–400 KB no banco;
  cada foto tem botão **"Amei"** (curtida com dedup por sessão) e contador de
  **visualizações**, com rankings no dashboard de analytics; as imagens cheias
  recebem **marca d'água** do portal e o endpoint `/arquivo/{id}` bloqueia
  **hotlink** de outros sites via Referer),
  hero com slides, seção de história com estatísticas, cultura & gastronomia,
  **vitrine das 7 Maravilhas** em **baralho de cartas com prévia** (carta central,
  próxima espreitando à direita, anterior à esquerda; arrastar/toque, setas no
  centro da foto, teclado ← → e contador — em `/lugares` a lista é uma grid
  numerada 01–07), **mapa interativo** com os POIs (filtros, legenda, modal com
  avaliações), agenda de eventos com exportação **.ics**, roteiros, notícias e
  rodapé com newsletter (LGPD) e **contatos com ícones por tipo** (endereço com
  pin, telefone/WhatsApp e redes sociais detectadas pela URL).
- **Mobile-first**: todo o portal é responsivo para celular (público principal)
  — grids em coluna única, navbar com menu hambúrguer, botão **voltar ao topo**
  e **preloader personalizado** com os 7 pictogramas das maravilhas.
- **CMS** com dois perfis (policies por **claim**, nunca roles literais):
  - **Gerenciador** — acesso total, organizado em **"Conteúdo do site"** com 10
    áreas (Hero, Nossa Cidade, Cultura, Gastronomia, 7 Maravilhas, Agenda,
    Notícias, Roteiros, Mapa, Rodapé) + **Galeria** (categorias dinâmicas com
    upload múltiplo de fotos otimizado no servidor): cada área mostra o que já está cadastrado,
    cadastra novos itens em modal (sem recarregar), edita os textos e imagens da
    área com seletor de chave e **prévia** da seção como fica no portal — além
    de **Tema e cores** (paleta de 6 cores editável sem recompilar SCSS),
    Configurações (guia/vídeo/SEO/logotipo), contatos, newsletter (busca + CSV +
    disparo em massa), moderação de avaliações e o **Dashboard de Analytics**
    (visitas, cliques, fontes de tráfego, rankings, newsletter e SEO).
    *Acessos são liberados pela DTI — este projeto não cria nem gerencia usuários.*
  - **Operador** — restrito a **Eventos** e **Newsletter**.
- **Analytics próprio** (anônimo, cookie de sessão `te_sessao`, sem dados
  pessoais): middleware rastreia visitas por rota/dispositivo e o portal envia
  cliques via `sendBeacon` para `POST /api/analytics/event`.
- **SEO**: sitemap dinâmico, `title`/`meta description` por página via
  `SeoService`, Open Graph e Twitter Cards, com `noindex` configurável.
- **Login próprio** (sem auto-registro público — acessos provisionados pela DTI).

## 📚 Documentação

| Documento | Conteúdo |
| --- | --- |
| [`docs/USO.md`](./docs/USO.md) | **Manual de uso** — como navegar no portal e operar o painel (Gerenciador/Operador) |
| [`docs/TECNICO.md`](./docs/TECNICO.md) | **Manual técnico** — arquitetura, fluxos, como adicionar módulos e manter |
| [`docs/superpowers/specs/2026-08-05-turismoestancia-design.md`](./docs/superpowers/specs/2026-08-05-turismoestancia-design.md) | Especificação de design (o que foi decidido na fase de brainstorming) |
| [`docs/superpowers/plans/2026-08-05-turismoestancia-plan.md`](./docs/superpowers/plans/2026-08-05-turismoestancia-plan.md) | Plano de implementação em fases |
| [`PADRAO-DE-PROJETO.md`](./PADRAO-DE-PROJETO.md) | Padrão de arquitetura genérico (Prefeitura Digital) que o projeto segue |

## 🚀 Como rodar

Pré-requisitos: **.NET 9 SDK**, **SQL Server** (LocalDB ou instância local).

```bash
# Subir o portal (migrações do banco principal aplicadas à parte)
dotnet run --project TurismoEstancia.Web
```

> ⛔ **Regra permanente:** rodar seed e alterar o banco do Identity são **proibidos**
> neste projeto (ver `PADRAO-DE-PROJETO.md` §7).

- **Portal:** `http://localhost:5xxx` (ver porta no console)
- **Painel:** `/Identity/Account/Login` (o acesso às áreas `/Gerenciador` e `/Operador`
  redireciona para o login)
- **Acessos ao painel** são liberados pelo **gerenciador geral de acessos da DTI** (sistemas
  do município) — este projeto **não cria nem gerencia usuários** (módulo Usuários removido;
  banco do Identity intocável, ver regras no `PADRAO-DE-PROJETO.md` §7).

## 🗄️ Bancos

| Banco                     | Connection string (appsettings.json)     | Conteúdo                    |
| ------------------------- | ---------------------------------------- | --------------------------- |
| `TurismoEstanciaDb`       | `TurismoEstancia`                        | Domínio do portal (negócio) |
| `TurismoEstanciaIdentityDb` | `TurismoEstanciaIdentity`              | ASP.NET Identity (usuários) |

Mídias (imagens, vídeo, guia) ficam em **byte[] no banco**, servidas por `GET /arquivo/{id}`.
A otimização de imagens usa **SixLabors.ImageSharp 3.1** e mora num lugar só: a regra de
`OtimizadorDeImagem` — 1600 px no maior lado, JPEG q82, rotação do EXIF aplicada aos pixels e
metadados (EXIF/GPS) descartados; PNG com transparência continua PNG. Ela vale para **todo
upload** do sistema (`IArquivoService.SalvarAsync`) e também para as miniaturas `?largura=N`,
que são geradas sob demanda pela mesma regra (o original é servido quando já é menor que o
pedido, e animação nunca é achatada); vídeo, PDF, SVG e GIF passam intactos.
A Galeria acrescenta a **marca d'água** (listras diagonais + logotipo no canto) para proteção
contra download.

O acervo que já está no banco (gravado antes desta regra) é recompimido pelo comando de
manutenção, que mostra o antes/depois em bytes:

```bash
# Mede o que faria, sem gravar nada
dotnet run --project TurismoEstancia.Web -- recomprimir-imagens --simular

# Aplica (de preferência com o portal parado)
dotnet run --project TurismoEstancia.Web -- recomprimir-imagens
```

É **idempotente** — rodar de novo não encontra nada para fazer — e **nunca aumenta** um
arquivo: se o re-encode ficar maior que o original, o original fica (por isso um PNG de
ícone já pequeno pode continuar acima de 1600 px). Nenhum registro é apagado. No fim, o
comando descarta tudo que é **derivado** (miniaturas de `?largura=N` e cópias locais de
vídeo), que senão continuariam mostrando a versão antiga.

O outro comando de manutenção é o que mostra o que está **sobrando** no acervo (arquivo
que nenhuma parte do sistema usa — nem por coluna de id, nem citado como texto numa seção):

```bash
# Só relata (nada é apagado)
dotnet run --project TurismoEstancia.Web -- arquivos-orfaos

# Relata e remove, junto com os derivados em disco
dotnet run --project TurismoEstancia.Web -- arquivos-orfaos --excluir
```
A regra de "quem usa esse arquivo" é a mesma que protege a exclusão feita pelo painel
(`ArquivoService.EstaReferenciadoAsync`, com a lista única de vínculos em `FontesDeId`), e a
remoção revalida cada arquivo antes de apagar: na dúvida, não apaga.

### Limites de upload e mídia

O limite é **por arquivo** (imagem 5 MB, vídeo 10 MB, outros 10 MB — `LimitesDeUpload`),
aplicado no ponto único de gravação e com mensagem no painel; há também um teto de
**transporte** de 60 MB por requisição (Kestrel/IIS/multipart), generoso porque um
formulário pode trazer várias fotos de uma vez — passar dele devolve o operador para a tela
com aviso, não uma página de erro.

**A mídia não passa por disco.** Toda leitura de `/arquivo/{id}` sai direto de
`Arquivos.ArquBytes`, em janelas de 1 MB (`FluxoDoArquivoNoBanco`): nada é gravado no
servidor — nem na pasta do deploy, nem no perfil do serviço —, o arquivo nunca é
materializado inteiro em memória e o `Range` funciona de verdade (a barra do player
arrasta), porque o fluxo é pesquisável. A janela vem alugada de um pool de buffers, e é o
que mantém o consumo baixo: medido com **20 downloads simultâneos de um vídeo de 15,4 MB**, o
processo subiu 33 MB (contra ~294 MB se cada requisição materializasse o blob).

As versões reduzidas de `?largura=N` (fotos, 200–2560 px) são derivadas com a mesma regra do
upload e ficam no **cache de memória do processo** (30 min, teto de 96 MB compartilhado com o
sitemap), então o acerto não consulta o banco. Duas consequências práticas: os comandos de
manutenção (`recomprimir-imagens`, `arquivos-orfaos`) devem rodar **com o portal parado** —
é o que descarta esse cache — e não há nada em disco para limpar ao publicar uma versão nova
(se existir `TurismoEstancia.Web/cache/` de instalações antigas, pode ser apagado: o código
atual não lê essa pasta).

O **vídeo do hero** é servido por esse mesmo streaming direto do banco: o blob não passa
inteiro pela memória a cada requisição, o `Range` funciona de verdade (a barra do player
permite arrastar) e o acerto não materializa nada. O **poster** (a
imagem que aparece antes de o vídeo começar) é extraído do próprio MP4 **no navegador do
operador**, no momento do envio — não depende de ffmpeg nem de qualquer passo no deploy — e
gravado como configuração `video-institucional-poster`; sem poster, o hero usa o 1º slide.

A tabela `Arquivos` segue o padrão **`PrefeituraDigital.Arquivo`** (colunas `ArquId`,
`ArquUID` ROWGUIDCOL, `ArquFileName`, `ArquContentType`, `ArquSize`, `ArquBytes`
`varbinary(max)`, `ArquMomento`, `ArquAutor`, `ArquAtivo`, `ArquOrigem`) e está **pronta
para FILESTREAM**: quando o filegroup for criado no servidor, basta executar
[`Deploy/01-Filestream-Config.sql`](./Deploy/01-Filestream-Config.sql) para converter o
`ArquBytes` em `varbinary(max) FILESTREAM` — sem nenhuma mudança de código (o EF já
lê/grava o binário da mesma forma).

Os **ícones** também são locais: `wwwroot/img/icones.svg` é um sprite com os ícones Lucide que o
sistema usa (37 KB, servido em 7,7 KB brotli) e `wwwroot/js/lucide-local.js` substitui a biblioteca
da CDN (**414 KB por página**) mantendo a mesma chamada `lucide.createIcons()` — inclusive o
comportamento que o seletor de ícones do painel depende (nome desconhecido não é substituído).
Para adicionar um ícone: use o nome no markup (`<i data-lucide="nome">`) ou nas listas do
`painel.js` e rode `python tools/gerar-sprite-icones.py` — o sprite e a lista de nomes do JS
se atualizam no mesmo passo (os aliases deprecados do Lucide, como `home` e `plus-circle`,
também entram).

## 🏗️ Arquitetura

```
TurismoEstancia.slnx
├── TurismoEstancia.Web/            # Entrada (MVC + Razor Pages + Areas)
│   ├── Areas/Gerenciador/          # CMS (Conteúdo do site em 10 áreas + CRUDs)
│   ├── Areas/Operador/             # Eventos + Newsletter
│   ├── Components/                 # ViewComponents: LogoSite, ThemeSite, ContatosRodape
│   ├── Controllers/                # Portal público + endpoints (.ics, arquivo, newsletter, avaliação)
│   ├── Pages/                      # Notícias e Roteiros (Razor Pages)
│   ├── Views/Home/                 # 11 partials do portal
│   └── wwwroot/scss/               # SCSS do portal (compila p/ css no build)
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
- **Seed é proibido** (regra permanente) — dados de referência entram por migração de
  schema, nunca por `HasData`/seeder.

## 🧪 Validação

- Build da solução: `dotnet build TurismoEstancia.slnx` (0 erros / 0 avisos).
- Smoke tests executados nas fases 5–7: portal 200, login 302→dashboard, CRUD
  criando registros no banco, restrição do Operador (admin → AccessDenied),
  exportação `.ics` e newsletter persistindo com consentimento LGPD.

## 📌 Roadmap v1.1 (sugestões)

- Redefinição de senha de usuários (e-mail) e edição de perfil.
- Edição dos **itens de roteiro** no CMS (hoje são dados de referência no banco).
- Paginação nas listagens do painel (Newsletter/Avaliações).
- Upload do **guia em PDF real** (hoje a configuração `guia-pdf` aceita qualquer arquivo; um PDF válido substitui a imagem de exemplo).
- Garantir **mínimo de 3 slides** no banco (a home só mostra as fotos polaroid
  da "Nossa Cidade" com 3+; com menos, a seção degrada sem quebrar).
- Dashboard de analytics: filtros por período além de 7/30/90 dias e
  exportação de relatórios.
- Testes automatizados (xUnit) para os serviços de negócio e controllers.
