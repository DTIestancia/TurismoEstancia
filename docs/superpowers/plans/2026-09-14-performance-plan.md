# Plano de Performance — TurismoEstância

> **Data:** 2026-09-14 · **Base:** branch `dev` · **Alvo:** `net9.0`
> **Origem:** análise do carregamento do portal e do painel (queixa: "está bem lento,
> talvez pela quantidade de imagens, mas pode ser outra coisa").
> **Escopo:** análise + planejamento. Os ganhos rápidos de cliente (itens 2.6, 3.1, 3.5 e 3.6)
> já foram implementados em 2026-09-14 — ver *Progresso* no §1; os demais itens seguem pendentes.

---

## 1. Resumo executivo

A suspeita de que "é a quantidade de imagens" está **parcialmente certa**, mas as imagens
são o **segundo** gargalo. O primeiro é **arquitetural e invisível**: a home executa
**~18 idas ao banco em série** a cada visita, e **nenhuma** página do portal usa cache de
página. Ou seja, cada visitante paga do zero toda a composição da página contra um
SQL Server remoto (`sqlserver01.estancia.local`).

Sobre as imagens, o diagnóstico é mais preciso do que "tem muitas": **elas são servidas
do banco (`byte[]`) a cada requisição, e só a Galeria otimiza a imagem no upload** —
todos os outros 12 módulos gravam o arquivo **original, sem resize nem re-encode**. O
alívio atual vem de um cache de miniaturas **em disco, gerado sob demanda** (`?largura=N`),
que faz a **primeira** visita de cada imagem pagar decode + resize + encode no servidor.

Há ainda dois agravantes de percepção: o **preloader** mantém a tela coberta por um tempo
**garantido** (~0,5 s + fade) mesmo quando a página já está pronta, e o **vídeo do hero**
`preload="auto"` + `fetchpriority="high"` compete por banda com o conteúdo acima da dobra.

**As três alavancas de maior retorno, em ordem:**

| # | Alavanca | Ganho esperado |
| --- | --- | --- |
| 1 | Cache de página/fragmento no portal (o conteúdo só muda quando o CMS salva) | remove ~18 consultas de ~95% das visitas |
| 2 | Otimizar **todas** as imagens no upload + `srcset`/`sizes` no portal | −60% a −80% de bytes de imagem no celular |
| 3 | `Cache-Control` nos estáticos + cortar o peso de vídeo/JS/fontes terceiras | −50% a −70% de bytes no primeiro acesso |

### Progresso dos ganhos rápidos (2026-09-14)

Primeira leva executada — só itens de cliente, sem tocar no desenho do backend:

| Item | Antes | Depois | Verificação |
| --- | --- | --- | --- |
| PNGs do rodapé (6) | 1.558 KB | **251 KB** (−84%) | comparação visual em tamanho real (preview) |
| `seloDiamante` + `seloEmpreendedor` | 1.357 KB | **89 KB** (−93%) | idem — imagem de 1.080 px exibida a 64 px |
| `Cache-Control` dos estáticos | nenhum (só ETag) | `max-age=31536000, immutable` com `?v=` / `max-age=86400` sem | `curl -D-` no app rodando |
| Vídeo do hero | `preload=auto` + `<link rel=preload as=video>` | `preload=none` + poster (1º slide) + play após o 1º paint | build + revisão do JS |
| Preloader | 500 ms fixos + 400 ms + 800 ms | sai quando o poster do hero carrega (teto de 2,5 s) | revisão do JS |
| Otimização no upload (todos os módulos) | original cru (7,9 MB num JPEG de 3200 px) | 1600 px, JPEG q82, sem EXIF/GPS | harness: 735 KB, EXIF ausente, rotação aplicada |
| Acervo legado (comando de manutenção) | 19,2 MB (4 fotos cruas) | **2,0 MB** (−89,5%) | 2ª execução: hash idêntico, nenhuma gravação |
| Miniaturas `?largura=N` | segunda implementação de resize/encode, PNG sempre que a origem era PNG e EXIF/GPS dentro do derivado | mesma regra do upload: rotação aplicada, sem EXIF, JPEG q82 (PNG só com alfa), animação preservada | foto de 900x650 com `Orientation=6` → 289x400 sem EXIF; PNG de 2400x1600 → JPEG de 21 KB |

Dois achados novos durante a execução:

1. **Os PNGs de `wwwroot/img/*` não estão no git** (`.gitignore:48` ignora tudo, só o
   `logo-barco-de-fogo.svg` é versionado) — ou seja, reduzi-los é irreversível sem backup.
   Por isso os originais ficaram em `wwwroot/img/originais/` (ignorados pelo git e removidos
   do publish por `<Content Remove="wwwroot\img\originais\**" />` no `.csproj`), junto de uma
   página `comparacao.html` com o antes/depois. **Expostos apenas em Development** (estão em
   `wwwroot`), nunca em produção.
2. **O CSS não está sendo minificado**: o `main.css` do pacote Release sai com **211.938 bytes**,
   idêntico ao build de desenvolvimento, e o `main.css.map` (40 KB) vai junto — confirmando o
   conflito de configuração apontado no item 3.4. O publish já gera `.br`/`.gz` (main.css.br =
   26.959 bytes), então há ~185 KB de CSS cru em cada publicação à toa.

---

## 2. Diagnóstico (evidências)

### 2.1 Servidor — o que pesa no TTFB

**a) ~18 consultas seriais por page view na home.** `HomeController.Index`
(`TurismoEstancia.Web/Controllers/HomeController.cs:70-114`) encadeia `await` um após o
outro: eventos (1), conteúdos (2), configurações (3, carrega tudo), slides (4), estatísticas
(5), grupos (6), pratos (7), tags (8), roteiros (9 + itens), planeje categorias (10),
planeje itens (11), notícias (12, **todas** as publicadas), contatos (13), categorias (14),
pontos (15) + mídias dos pontos (16), conheça estância (17). Somam-se o ViewComponent de
contatos do rodapé (18) e o `LogoSite`. Com 8–20 ms por ida e volta, são **~150–360 ms só
de latência acumulada**, antes de qualquer render.

> ⚠️ Não dá para "só paralelizar com `Task.WhenAll`": `AppDbContext` é **scoped e não é
> thread-safe** — dispara `A second operation was started on this context`. Paralelizar
> exige `IDbContextFactory<AppDbContext>` (um contexto por task) ou, preferencialmente,
> **deixar de executar a composição a cada request** (item 1 do plano).

**b) Queries que trazem a coleção inteira e paginam em memória.**
- `NoticiaService.ListarAsync` traz todas as publicadas; o controller faz `.Take(3)` depois
  (`HomeController.cs:99`) e `/Noticias` pagina com `Skip/Take` **em memória**
  (`Pages/Noticias/Index.cshtml.cs:39-43`).
- `GaleriaService.ListarFotosTodasAsync` carrega **todas** as fotos ativas de **todas** as
  categorias, deduplica com `GroupBy` em memória e o controller ordena/pagina em memória
  (`Controllers/GaleriaController.cs:60-70`) — O(n) por visualização de página.
- Igual em eventos (`.Take(3)` em memória) e no `sitemap.xml` (varre 7 serviços).

**c) `ConfiguracaoSiteCache` é cache **por request**, não compartilhado**
(`Infrastructure/ConfiguracaoSiteCache.cs`). Toda página paga 1 query extra de
configurações — e essa query está no caminho crítico do `<head>` (favicon), do SEO, do
logotipo e do tema. O `AddMemoryCache()` já está registrado
(`Extensions/InfrastructureExtensions.cs:19`) mas só o `SeoController` usa.

**d) Upload sem otimização em 12 de 13 caminhos.** Só a Galeria chama
`SalvarImagemOtimizadaAsync`/`SalvarThumbnailAsync`. Slides (`SlideService.cs:53,71`),
Notícias (`NoticiaService.cs:102,128`), Pontos turísticos (`PontoTuristicoService.cs:140,
173,297`), Conheça Estância, Grupos, Pratos, Tags, Roteiros, Planeje, ícones de categoria e
**configurações** (mapa, `historia-imagem`, `hero-titulo-imagem`) gravam os **bytes
originais**. Uma foto de celular de 5 MB vira 5 MB no banco. O `?largura=` existente é a
única rede de proteção — e ela é **gerada sob demanda**.

**e) Cache de miniaturas frágil.** `ArquivoController` (`Controllers/ArquivoController.cs:53-96`)
grava em `{ContentRoot}/cache/arquivo/{id}-{largura}.jpg` no primeiro acesso: a **primeira**
visita de cada imagem + largura paga decode/resize/encode (CPU-bound, segurando um
`SemaphoreSlim` por chave). Não há pré-aquecimento, não há limite/purge, **não é limpo ao
excluir o arquivo** e é perdido a cada re-deploy. Em duas instâncias IIS, o cache fica em
disco local (cada nó aquece o seu).

**f) Painel: overhead por página.**
- `_PainelLayout.cshtml:7` chama `await UserManager.GetUserAsync(User)` → **1 query no
  banco do Identity (servidor separado)** em **toda** página do painel.
- `OperadorController.OnActionExecutionAsync` (`Areas/Operador/Controllers/OperadorController.cs:37-43`)
  **abre um novo `IServiceScope` a cada action** só para contar avaliações pendentes —
  conexão extra do pool + query, inclusive em POSTs.
- Nenhuma middleware/policy de output cache; toda lista do CMS re-consulta tudo.

**g) Conexão.** `MultipleActiveResultSets=true` nas duas connection strings
(`appsettings.json`) — MARS tem custo e costuma mascarar consultas mal ordenadas. Não há
`Max Pool Size`, `Connection Timeout` nem `Command Timeout` explícitos.

### 2.2 Cliente — o que pesa no carregamento

**a) Vídeo do hero no caminho crítico.** `_Hero.cshtml:114` e `Home/Index.cshtml:7-17`
declaram `preload="auto"`, `fetchpriority="high"` e um `<link rel="preload" as="video">`.
O MP4 (vindo do banco, por `/arquivo/{id}` sem `?largura`) disputa banda com CSS, fontes e
a própria imagem de LCP. Em 3G/4G de interior isso domina o tempo de "site carregando".

**b) Sem `Cache-Control` nos estáticos.** `app.UseStaticFiles()` sem `OnPrepareResponse`
→ só `Last-Modified`/`ETag`. Todo arquivo é **revalidado a cada navegação**
(main.css 212 KB no disco, portal.js 58 KB, painel.js 23 KB, PNGs do rodapé).

**c) Assets estáticos pesados / mal gerados.**
| Arquivo | Tamanho | Observação |
| --- | --- | --- |
| `wwwroot/img/seloDiamante.png` | **1,1 MB** | exibido em ~100–150 px (`_PaginasFooter.cshtml`) |
| `wwwroot/img/seloEmpreendedor.png` | 268 KB | idem |
| `wwwroot/lib/quill/quill.min.js` | 210 KB | só o painel usa (`_EditorTextoRico.cshtml`) |
| `wwwroot/css/main.css` | 212 KB (+ `main.css.map` 40 KB) | `.map` **é publicado** (está em `wwwroot`) |
| `wwwroot/js/portal.js` | 58 KB | sem minificação |
| lucide (CDN) | **414 KB** | para renderizar ~40 ícones |

**d) Terceiros no caminho crítico do `<head>`** (`Views/Shared/_Layout.cshtml:105-110`):
Google Fonts com **4 famílias** em um único `<link>` render-blocking, `preconnect` para
`cdnjs.cloudflare.com` (**nada é carregado de lá** — verificado por grep) e lucide do
jsdelivr. Cada domínio novo = DNS + TLS + RTT. (Verifiquei: `lucide@1.27.0` existe no
jsDelivr, responde 200 — não é um link quebrado, é só caro.)

**e) Sem imagem responsiva nem reserva de espaço.** Não há `srcset`/`sizes` e quase nenhum
`width`/`height`. O celular (público principal, por design) baixa `?largura=1600`, `=1920`
e `=1200` conforme a seção, e cada `<img>` sem dimensão gera CLS.

### 2.3 Percepção — o que "parece" lento sem ser

O **preloader** (`wwwroot/js/portal.js:8-27`) só chama `completeBar` **500 ms depois** do
`DOMContentLoaded`, depois espera 400 ms para adicionar `.hidden` e mais 800 ms para remover
o nó. Como `portal.js` está no fim do `<body>` **sem `defer`/`async`**, o `DOMContentLoaded`
já vem depois do download do script. Resultado: **≥0,5 s de tela coberta garantidos** mesmo
em cache quente, mais o fade. Em conexão lenta esse tempo é o dobro do necessário.

### 2.4 Infra

- **Data Protection com chaves efêmeras**: `InfrastructureExtensions.cs` deixa explícito que
  `PersistKeysToDbContext` "fica para a Fase 2". Há **dois servidores IIS**
  (`webserver01iis.pubxml`, `webserver02iis.pubxml`): sem chaves compartilhadas e
  persistentes, cookies de autenticação e tokens antiforgery quebram entre nós e a cada
  reciclagem do pool (não é lentidão, é indisponibilidade intermitente — mesma família de
  problema, e aparece junto do "está lento").
- **Arquivos em `varbinary(max)`** pressurizam o buffer pool: cada `/arquivo/{id}` carrega o
  blob inteiro para a memória do processo. O `Deploy/01-Filestream-Config.sql` já está pronto
  e o EF não muda.
- Sem warm-up no IIS (`preloadEnabled`/Application Initialization) → primeira requisição
  pós-reciclagem paga JIT + compilação.

---

## 3. Fase 0 — Medir antes de mexer

Sem baseline, não há como provar ganho. Tudo abaixo é barato e reversível.

- [ ] 0.1 **Baseline de TTFB**: 5 cargas em cache frio e 5 em cache quente da home, `/galeria`,
      `/noticias` e `/Gerenciador/Dashboard`; registrar TTFB, `DOMContentLoaded` e `Load`
      (DevTools → Network, coluna *Time*).
- [ ] 0.2 **Baseline de bytes**: Home → Network → desabilitar cache → anotar (a) total
      transferido, (b) nº de requisições, (c) nº de requisições para `/arquivo/*`,
      (d) maior recurso.
- [ ] 0.3 **Baseline de banco**: contar as consultas por request. Caminho rápido e sem
      alterar produção: `appsettings.Development.json` → `Logging:LogLevel:Microsoft.EntityFrameworkCore.Database.Command = Information`
      e contar as linhas `Executed DbCommand` de um único GET `/`.
- [ ] 0.4 **Middlewares de diagnóstico (dev)**: um `Server-Timing` com o tempo total e o nº de
      queries do EF, emitido só em Development — vira o "placar" permanente das fases seguintes.
- [ ] 0.5 **Orçamento acordado (metas)**:
      | Métrica | Hoje (estimado) | Meta |
      | --- | --- | --- |
      | TTFB home (cache quente) | ~250–500 ms | < 120 ms |
      | TTFB home (cache frio) | ~400–800 ms | < 300 ms |
      | Queries por GET / | ~18 | ≤ 2 |
      | Peso home mobile (1ª visita) | vários MB | < 1,2 MB |
      | LCP mobile 4G | a medir | < 2,5 s |
      | CLS | a medir | < 0,1 |

---

## 4. Fase 1 — Backend: parar de recompor a página a cada request (maior ROI)

- [x] 1.1 **Cache de página no portal público** com `AddOutputCache` — **feito (2026-09-14)**.
      `Infrastructure/CachePortalPublicoPolicy.cs` decide o que entra: **só** GET/HEAD de
      página pública com visitante anônimo (mesma definição de `Infrastructure/RotasPortal.cs`
      que o analytics usa), `AllowLocking` ligado, `MaximumBodySize` de 1 MB, validade de 5 min
      e `QueryKeys = pagina, paginaPassados` (galeria, notícias e os dois paginadores da agenda).
      `ServeResponseAsync` descarta o que não deu 200, para uma varredura de URLs inexistentes
      não encher a memória com páginas de erro.
      Medido no app rodando (banco local descartável): home **18 consultas e 1,01 s de TTFB** na
      primeira carga → **0 consultas de página e 5 ms** (`Age: 2`) nas seguintes; `/noticias`
      **5 → 0 consultas**; todas as rotas públicas testadas servem com `Age`.
      Duas armadilhas que só apareceram no teste real:
      - **O cache guarda cabeçalhos**, e as páginas do portal emitem cookies do visitante
        (sessão anônima + anti-forgery): sem tratamento, todos os visitantes recebiam a sessão
        e o token do primeiro (visitante único colapsa; e o token+cookie públicos permitem
        forjar o POST de outro visitante). `Middleware/CookiesDeVisitanteMiddleware.cs`
        resolve na saída (`OnStarting`, a única janela em que os cabeçalhos ainda aceitam
        mudança — nesta página o Razor já descarregou a resposta durante a renderização).
        Verificado: cada request recebe um `te_sessao` próprio e o cookie anti-forgery do
        cache não sai mais.
      - Como o anti-forgery sai do HTML cacheado, **o POST dos formulários públicos
        (newsletter/avaliação) depende do JS** que troca o token no carregamento
        (`Controllers/AntiforgeryController.cs` + `wwwroot/js/portal.js`). Sem JS, o envio
        falha na validação anti-forgery — era o preço de não compartilhar o token entre
        visitantes.
- [x] 1.2 **Invalidação ao salvar no CMS** — **feito (2026-09-14)**, por tag em vez de versão de
      conteúdo: `Middleware/InvalidaCacheConteudoMiddleware.cs` observa a resposta de todo POST
      em `/Gerenciador`/`/Operador` — terminou sem erro e com usuário logado → `EvictByTagAsync`
      descarta o portal inteiro de uma vez (uma tag só, `conteudo-portal`). Nenhuma ação do
      painel precisa lembrar de chamar nada: vale para qualquer tela futura.
      Provado com o app rodando: home em `Age: 1` → POST no painel → próxima carga volta a
      renderizar (SEM `Age`) e a seguinte já é acerto de novo. POST anônimo no painel (302 para
      o login) **não** derruba o cache.
- [ ] 1.3 **Substituir o cache por request do `ConfiguracaoSiteCache` por cache compartilhado**
      (`IMemoryCache` já registrado) com a mesma invalidação de 1.2. Isso remove a query de
      configurações de **toda** página do portal **e** do painel (favicon, SEO, tema, logotipo,
      rodapé).
- [ ] 1.4 **Mover o cache de contatos do rodapé** de `HttpContext.Items` para o cache
      compartilhado (mesma invalidação). O `ContatosRodapeViewComponent` já está preparado.
- [ ] 1.5 **Empurrar paginação e limites para o SQL**:
      - adicionar `limite`/`skip`/`take` em `INoticiaService.ListarAsync` e usar `Take(3)` **na query**;
      - `IGaleriaService.ListarFotosTodasAsync` → contagem (`COUNT DISTINCT ArquivoId`) + página
        (`GROUP BY ArquivoId` + `MIN` + `OrderByDescending(Visualizacoes)` + `Skip/Take`);
      - `IEventoService.ListarAsync(apenasProximos)` com `Take` no SQL.
- [ ] 1.6 **Home opcionalmente paralela** (se 1.1/1.2 não forem adotados de imediato): registrar
      `AddDbContextFactory<AppDbContext>` **além** do `AddDbContext` e, no `HomeController`,
      compor as seções com `Task.WhenAll` sobre contextos independentes. Um único round trip de
      latência em vez de 18.
- [ ] 1.7 Remover as consultas de `Include` redundantes nas projeções (`PontoTuristicoService`,
      `MidiaKitService`) — em projeção o `Include` é ignorado e só confunde.
- [ ] 1.8 Avaliar **índices de leitura** no `AppDbContext` (nova migração, permitida — só seed e
      banco do Identity são proibidos): `Noticias (Ativo, Publicada, DataPublicacao)`,
      `GaleriaMidias (CategoriaId, Ativo, Ordem)`, `Eventos (Ativo, DataInicio)`,
      `AnalyticsEventos (Tipo, Data)`.

## 5. Fase 2 — Imagens (o suspeito do usuário)

- [x] 2.1 **Otimizar no upload em todos os módulos** — **feito (2026-09-14)**. A regra virou uma
      só, em `Services/Infra/Imagens/OtimizadorDeImagem.cs` (1600 px no maior lado, JPEG q82,
      EXIF/IPTC/XMP fora, alfa preservado), e entrou no **ponto único de gravação**
      (`IArquivoService.SalvarAsync`): os 12 módulos não precisaram mudar uma linha, e o
      Logotipo/favicon/mídia kit continuam intactos. Medido: 7,9 MB → 735 KB (−90%), PNG
      11,5 MB → 899 KB (−92%), PNG com transparência segue PNG.
- [x] 2.2 **Recomprimir o acervo já existente** — **feito (2026-09-14)**: comando
      `dotnet run --project TurismoEstancia.Web -- recomprimir-imagens [--simular]`, que percorre
      `Arquivos` de 50 em 50 (nunca carrega o acervo em memória) e relata o antes/depois.
      **Idempotente** — a 2ª execução não grava nada (hash de todas as linhas idêntico) e a foto
      já no formato final é reconhecida só pelo cabeçalho, sem decodificar pixels.
      **Nunca aumenta** um arquivo: se o re-encode ficar maior, o original fica (é o caso de um
      PNG de ícone com alfa já pequeno, que pode seguir acima de 1600 px). Medido: 19,2 MB →
      2,0 MB (−89,5%) e 3 fotos do acervo legado recompimidas.
      Decisão diferente da prevista no plano: em vez de criar registro novo e reapontar cada
      referência, a troca é feita **no mesmo registro** (`ExecuteUpdate`), porque o `ArquivoId`
      é a URL e a URL não muda; o ETag de `/arquivo/{id}` carrega o `Size`, então quem tem a
      versão antiga revalida e recebe a nova. O comando descarta as miniaturas derivadas de
      `cache/arquivo` (senão mostrariam a versão antiga para sempre) e é para rodar em janela de
      manutenção.
      Dois achados de biblioteca, ambos medidos: o `SkipMetadata` do `JpegEncoder` **não** remove
      o EXIF nesta versão do ImageSharp (foto com 3 campos EXIF saía com EXIF), então o perfil é
      zerado na mão; e o ImageSharp **não auto-orienta** ao carregar (arquivo com `Orientation=6`
      carrega na largura original) — descartar o EXIF sem mais nada deixaria a foto de celular
      deitada, então `AutoOrient()` roda antes do descarte (verificado: 900x650 com
      `Orientation=6` → gravado 650x900, sem EXIF).
- [ ] 2.3 **Pré-aquecer o cache de miniaturas** no startup (ou por comando) para as larguras
      realmente usadas na home e na galeria. Hoje a primeira visita de cada imagem paga
      decode+resize+encode.
- [ ] 2.4 **Higiene do cache em disco**: limite de tamanho/LRU, limpeza ao excluir o arquivo e
      diretório estável fora da pasta de deploy (ex.: `App_Data`/path em `appsettings`) para não
      zerar no re-deploy nem conflitar entre os dois nós IIS.
      **Atenção no deploy da unificação das miniaturas**: `LocalizarMiniatura` procura `.jpg` e
      depois `.png`, então as miniaturas geradas pela implementação anterior (que guardavam
      EXIF/GPS dentro do arquivo e já foram baixadas com `immutable` de um ano pelos visitantes)
      continuariam sendo servidas. Apagar `cache/arquivo` ao publicar — o comando
      `recomprimir-imagens` já faz isso — força a regeneração pela regra nova.
- [ ] 2.5 **`srcset`/`sizes` + `width`/`height`** em todos os `<img>` do portal, servindo
      400/800/1200/1920 conforme o viewport. É o ganho de bytes mais direto no celular e
      elimina o CLS.
- [x] 2.6 **Comprimir os PNGs estáticos** do rodapé — **feito (2026-09-14)**: 1.558 KB → 251 KB
      (−84%), com `seloDiamante` 1.090 KB → 53 KB e `seloEmpreendedor` 266 KB → 36 KB, para uma
      exibição de **64 px** de altura (origem de 1.080 px). Redimensionado para 3× a exibição
      (192 px) mantendo RGBA — a quantização para paleta foi descartada porque 0,9%–7,4% dos
      pixels têm alfa parcial (bordas serrilhariam contra o fundo escuro).
      Pendente: `logotipoPictoPref` e `CapitalBrasileira` **ficaram no original** porque o
      re-encode RGBA ficaria maior (58,5 KB e 38,3 KB contra 51,8 KB e 35,0 KB) — só o
      `pngquant`/`oxipng` (com alpha de verdade) resolve esses dois.
- [ ] 2.7 Avaliar **FILESTREAM** (`Deploy/01-Filestream-Config.sql`): tira os blobs do buffer
      pool e do tráfego do processo; o EF não muda.
- [ ] 2.8 Avaliar cache HTTP/CDN reverso na frente de `/arquivo/*` (as respostas já são
      `immutable` + ETag). Atenção: a proteção anti-hotlink por `Referer` retorna 403 — o proxy
      precisa **não** cachear 403 nem perder o `Referer`.

## 6. Fase 3 — Rede, assets e percepção

- [x] 3.1 **`Cache-Control` nos estáticos** — **feito (2026-09-14)**: `UseStaticFiles` com
      `OnPrepareResponse` → `public, max-age=31536000, immutable` quando há `?v=` e
      `public, max-age=86400` no resto. Verificado com `curl -D-`: `/css/main.css` responde
      `86400`, `/css/main.css?v=abc123` e `/js/portal.js?v=1` respondem `immutable`.
- [ ] 3.2 **Self-host de fontes e ícones**: servir os `woff2` (subset pt-BR) em `wwwroot/fonts`
      e o lucide local — ou melhor, **substituir o lucide por um sprite SVG** com os ~40 ícones
      realmente usados (414 KB → poucos KB). Remove 2 domínios terceiros do caminho crítico.
- [ ] 3.3 Remover o `preconnect` para `cdnjs.cloudflare.com` (não é usado).
- [ ] 3.4 **Minificar `portal.js` e `painel.js`** no publish (esbuild/terser via target MSBuild) e
      garantir `--style=compressed` em Release. **Conflito confirmado em 2026-09-14**: o
      `main.css` publicado em Release tem **exatamente 211.938 bytes** — igual ao build de
      desenvolvimento — e o `main.css.map` (40.644 bytes) vai no pacote. `appsettings.json` tem
      `SassCompiler:Arguments = "--style=expanded --no-source-map"`, que está vencendo o
      `Release → --style=compressed` do `sasscompiler.json`. Ação: remover a seção
      `SassCompiler` do `appsettings.json` (o pacote lê o `sasscompiler.json`) e
      **excluir os `.map` do publish** (`<Content Remove="wwwroot/css/*.map" />`).
- [x] 3.5 **Hero** — **feito (2026-09-14)**: `preload="none"` + `poster` (1º slide, via
      `HomeViewModel.HeroPosterArquivoId`), `<link rel=preload as=image fetchpriority=high>` no
      lugar do preload de vídeo, e `portal.js` disparando o `play()` dois `requestAnimationFrame`
      depois do DOM pronto (com `visibilitychange` para aba em segundo plano e teto de 3 s).
      Pendente: em mobile, servir imagem em vez de vídeo.
- [x] 3.6 **Preloader** — **feito (2026-09-14)**: os 500 ms fixos + 400 ms + 800 ms saíram. O
      preloader agora sai quando o **poster do hero** termina de carregar (a imagem já está em voo
      pelo preload, então não há download duplicado) e tem teto de 2,5 s para rede ruim. Isso
      evita o efeito colateral de revelar um hero vazio.
      Pendente: `defer` no `portal.js` (mover para o `<head>` exige conferir a ordem com os
      scripts inline da seção `Scripts`, que dependem de `window.turismoEstancia`).
- [ ] 3.7 Revisar `BrotliCompressionProviderOptions.Level = Fastest`
      (`Program.cs:16`): subir para `Optimal` (a CPU extra é desprezível para HTML/CSS/JS e o
      ganho de bytes é real), e confirmar que as mídias estão fora da compressão.

## 7. Fase 4 — Painel (CMS)

- [ ] 4.1 Trocar `UserManager.GetUserAsync(User)` no `_PainelLayout` por leitura das **claims já
      presentes no cookie** (o layout só precisa do nome e do perfil, que já estão em
      `User.FindAll(Perfis.TipoClaim)`) — remove 1 query ao banco do Identity por página.
- [ ] 4.2 **Remover o `CreateScope` por action** do `OperadorController` (e fazer o mesmo no
      equivalente do Gerenciador, se existir): usar o serviço scoped já injetado, ou mover o
      contador para o cache compartilhado com TTL curto, ou desenhar o badge por AJAX.
- [ ] 4.3 Paginação em SQL nas listagens grandes (Newsletter, Avaliações, Analytics) — já está no
      Roadmap v1.1 do README, e é aqui que ela rende.
- [ ] 4.4 Carregar **Quill sob demanda** (`_EditorTextoRico`): hoje 210 KB de JS + 25 KB de CSS
      entram em toda tela de formulário; carregue no `focus`/`click` do campo ou só nas telas que
      realmente têm editor.

## 8. Fase 5 — Infra / IIS

- [ ] 5.1 **Persistir as chaves do Data Protection** (`PersistKeysToDbContext` no banco do
      domínio, nunca no Identity) + `SetApplicationName` fixo. Resolve invalidação de login e de
      antiforgery entre `webserver01`/`webserver02` e a cada reciclagem.
- [ ] 5.2 **Warm-up** no IIS: `preloadEnabled`/Application Initialization apontando para uma rota
      leve, `startMode=AlwaysRunning`, e desativar a reciclagem por ociosidade — mata o "primeiro
      acesso do dia está lento".
- [ ] 5.3 **Revisar a connection string**: remover `MultipleActiveResultSets=true` (se nada
      depender dele), definir `Max Pool Size` e `Connect Timeout`; garantir `Encrypt`/
      `TrustServerCertificate` coerentes com produção.
- [ ] 5.4 Confirmar em qual **filegroup** a tabela `Arquivos` está e se o `.mdf` cresceu por
      causa dos blobs — insumo para decidir o FILESTREAM (2.7).
- [ ] 5.5 *(Opcional, se houver tempo)* mover a camada de release para um **reverse proxy**
      (nginx/IIS ARR) com cache de estáticos e `gzip_static`/Brotli pré-comprimido.

## 9. Fase 6 — Guardrails (para não regredir)

- [ ] 6.1 **Orçamento de performance no `dotnet build`/CI**: um teste que falhe se a home
      renderizar com mais de N queries (contador do EF) ou mais de X requisições `/arquivo`.
- [ ] 6.2 **Smoke de performance** com `WebApplicationFactory` (xUnit) medindo TTFB da home com
      cache quente/frio e tamanho do HTML — o README já pede testes automatizados no Roadmap v1.1.
- [ ] 6.3 **Lighthouse CI** com limites de LCP/CLS/peso, rodando contra a home e `/galeria`.
- [ ] 6.4 Documentar as duas regras que evitam a recaída: (a) **todo upload de imagem passa por
      `SalvarImagemOtimizadaAsync`**; (b) **todo `<img>` novo tem `srcset`/`sizes` e dimensões**.

---

## 10. Ordem de execução sugerida

| Ordem | Item | Impacto | Esforço |
| --- | --- | --- | --- |
| 1 | 3.1 `Cache-Control` nos estáticos | alto | baixo |
| 2 | 1.3 + 1.4 cache compartilhado (config/contatos) + invalidação | alto | baixo |
| 3 | 3.6 preloader + `defer` | percepção alta | baixo |
| 4 | 3.2 self-host de lucide/fontes | alto | médio |
| 5 | 3.5 hero: poster + vídeo pós-paint | alto (banda) | baixo |
| 6 | 2.6 PNGs do rodapé + 3.4 minificar/excluir `.map` | alto | baixo |
| 7 | 1.1 + 1.2 output cache com invalidação por versão | **muito alto** | médio |
| 8 | 2.1 otimizar todos os uploads | alto | médio |
| 9 | 2.5 `srcset`/`sizes` + dimensões | alto | médio |
| 10 | 1.5 paginação em SQL | médio | médio |
| 11 | 4.1 + 4.2 overhead do painel | médio | baixo |
| 12 | 2.3 pré-aquecimento de miniaturas | médio | médio |
| 13 | 5.1 + 5.2 Data Protection + warm-up IIS | confiabilidade | baixo |
| 14 | 2.2 recomprimir acervo existente | muito alto | alto (tem dados) |
| 15 | 2.7 FILESTREAM | médio | alto (infra) |
| 16 | Fase 6 guardrails | evita regressão | médio |

---

## 11. O que não fazer / riscos

- **Não paralelizar consultas no mesmo `AppDbContext`.** É o erro mais provável e mais fácil
  de cometer ao atacar o item 2.1.a: dá exceção em runtime sob concorrência. Use
  `IDbContextFactory` ou cache de página.
- **Não cachear resposta de usuário autenticado.** O painel usa `[Authorize]`, `TempData` e
  antiforgery por sessão; output cache ali vaza dado entre usuários. Limite o cache ao portal
  anônimo.
- **Não mexer no banco do Identity nem rodar seed** — regras permanentes do
  `PADRAO-DE-PROJETO.md` §7, sem exceção para validar performance.
- **Não reescrever bytes de `Arquivos` in-place** sem planejar as referências: o contrato atual
  é "arquivo imutável, troca = novo registro + reaponta". Um `UPDATE` direto em `ArquBytes`
  pode invalidar o cache de miniaturas (o nome do arquivo de cache é `{id}-{largura}`, e o ETag
  usa `CriadoEm.Ticks`, que **não muda** em update) → servir miniatura velha indefinidamente.
  Se 2.2 for in-place, **limpar o cache em disco** e versionar o ETag por hash do conteúdo.
- **Não aumentar o TTL de cache sem invalidação.** Conteúdo editado no CMS deve aparecer na
  hora; cache sem eviction é bug de conteúdo, não ganho de performance.
- **Medir antes e depois de cada fase.** Sem o baseline da Fase 0, o ganho vira opinião.

---

## 12. Anexo — inventário de achados

| # | Achado | Severidade | Local |
| --- | --- | --- | --- |
| 1 | ~18 consultas seriais por GET `/` | **Alta** | `Controllers/HomeController.cs:70-114` |
| 2 | Nenhum cache de página/fragmento no portal | **Alta** | `Extensions/PipelineExtensions.cs` |
| 3 | Cache de config por request, não compartilhado | Alta | `Infrastructure/ConfiguracaoSiteCache.cs` |
| 4 | Miniaturas geradas sob demanda, sem warm-up/purge | Alta | `Controllers/ArquivoController.cs:53-96` |
| 5 | 12 de 13 uploads gravam imagem original | **Alta** | `Services/**/Services/*Service.cs` |
| 6 | Sem `Cache-Control` nos estáticos | Alta | `Extensions/PipelineExtensions.cs:38` |
| 7 | Vídeo do hero `preload=auto` + preload no `<head>` | Alta | `Views/Home/_Hero.cshtml:114`, `Views/Home/Index.cshtml:7-17` |
| 8 | 1,4 MB de PNGs de selo no rodapé | Alta | `wwwroot/img/selo*.png`, `Views/Shared/_PaginasFooter.cshtml` |
| 9 | lucide 414 KB de CDN para ~40 ícones | Alta | `Views/Shared/_Layout.cshtml:110` |
| 10 | Google Fonts 4 famílias render-blocking | Média | `Views/Shared/_Layout.cshtml:109` |
| 11 | Preloader com atraso mínimo fixo | Média (percepção) | `wwwroot/js/portal.js:8-27` |
| 12 | Sem `srcset`/`sizes`/dimensões nas imagens | Alta | partais `Views/Home/*`, `Views/Galeria/Index.cshtml` |
| 13 | Paginação/limites em memória (notícias, galeria, eventos) | Média | `Services/Comunicacao/Services/NoticiaService.cs:46-56`, `Services/Galeria/Services/GaleriaService.cs:171-205` |
| 14 | `main.css.map` publicado; CSS possivelmente expanded | Média | `wwwroot/css/`, `appsettings.json` (`SassCompiler`) |
| 15 | Quill 210 KB em toda tela com editor | Média | `Views/Shared/_EditorTextoRico.cshtml` |
| 16 | `UserManager.GetUserAsync` por página do painel | Média | `Views/Shared/_PainelLayout.cshtml:7` |
| 17 | `CreateScope` + query por action no Operador | Média | `Areas/Operador/Controllers/OperadorController.cs:37-43` |
| 18 | Data Protection efêmero com 2 nós IIS | Média (confiabilidade) | `Extensions/InfrastructureExtensions.cs:47-49` |
| 19 | `MultipleActiveResultSets=true` sem necessidade clara | Baixa | `appsettings.json` |
| 20 | `preconnect` para cdnjs não utilizado | Baixa | `Views/Shared/_Layout.cshtml:107` |
