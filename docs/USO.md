# 📖 Manual de Uso — TurismoEstância

Sistema de turismo de **Estância/SE — Capital Sergipana da Cultura**. Este manual explica
como usar o sistema do ponto de vista de quem opera o conteúdo (painel) e de quem visita
o portal.

---

## 1. Acessos

| O quê | Onde |
| --- | --- |
| **Portal público** | `http://localhost:5257` (ou a porta configurada) |
| **Painel (login)** | `/Identity/Account/Login` — ao acessar `/Gerenciador` ou `/Operador` sem login, o sistema redireciona para o login |
| **Usuário admin (criado pelo seed)** | `admin@estancia.se.gov.br` / `Estancia@2026` |

> ⚠️ Troque a senha em produção. O seed é **idempotente**: rodar de novo não duplica dados.
> Contas novas são criadas pelo **Gerenciador** (não há auto-registro público).

---

## 2. O Portal (lado do visitante)

### 2.1 Navegação

A home é uma página única com as seções:

1. **Hero** — vídeo/imagens de abertura, com o botão "Baixe o Guia".
2. **Nossa Cidade** — história, estatísticas e fotos.
3. **Nossa Cultura** — manifestações culturais e grupos.
4. **Grupos & Gastronomia** — grupos populares e pratos típicos.
5. **7 Maravilhas** — a vitrine de postais (veja abaixo).
6. **Mapa Interativo** — todos os pontos no mapa, com filtros e modal de detalhes.
7. **Agenda** — eventos com botão de adicionar ao calendário (`.ics`).
8. **Roteiros** — roteiros sugeridos de visitação.
9. **Notícias** — página própria em `/noticias`.
10. **Rodapé** — newsletter (LGPD), contatos e links.

O menu superior navega por âncoras. Em telas pequenas ele vira um **menu hambúrguer**
de tela cheia. Há um **botão "voltar ao topo"** flutuante no canto inferior direito
que aparece ao rolar.

### 2.2 A vitrine das 7 Maravilhas

- A seção "Lugares que encantam" mostra um **postal grande** com a foto da maravilha,
  categoria, descrição, tag e o botão **"Ver detalhes"** (leva à página completa do lugar).
- Os **postais menores** embaixo navegam o postal grande (troca de foto com fade).
  Funciona por **clique, toque, teclado (← →)** e **swipe**.
- A página `/lugares` repete a vitrine completa.

### 2.3 Mapa interativo

- Filtros por categoria, legenda e **modal de detalhes** ao clicar num marcador
  (foto, endereço, horários, "como chegar" e **avaliações**).
- Avaliações são deixadas pelo visitante e **só aparecem após aprovação no painel**.

### 2.4 Outros conteúdos

- **Agenda**: cada evento tem data/hora, local e botão **"Adicionar ao calendário"**
  (gera arquivo `.ics`).
- **Notícias** (`/noticias`) e **Roteiros** (`/roteiros`): listagens com páginas de
  detalhe.
- **Newsletter**: o visitante informa e-mail + nome e marca a caixa LGPD para receber
  novidades. O e-mail fica na lista do painel.

---

## 3. O Painel (Gerenciador)

O painel é o CMS do portal. Tudo que aparece no portal é editável aqui.

### 3.1 Dashboard (analytics)

Ao entrar, o **Dashboard** mostra:

- **Contadores de conteúdo** — quantos pontos, categorias, eventos, notícias,
  roteiros, grupos, pratos, inscrições e avaliações existem.
- **Analytics do portal** (período de **7, 30 ou 90 dias**):
  - Visitas por dia e por rota;
  - Cliques rastreados (ex.: "Ver detalhes", "Baixar guia", links externos);
  - Fontes de tráfego (referer);
  - Dispositivos (Desktop/Mobile/Tablet);
  - Rankings de pontos, notícias e eventos mais vistos.
- **Newsletter** — novas inscrições no período e total ativas.
- **SEO** — título e meta description atuais, além do total de **rotas indexáveis**
  no sitemap.

> Os dados são **anônimos** (cookie de sessão `te_sessao`, sem dados pessoais).
> O período é trocado pelos botões 7/30/90 dias na tela.

### 3.2 Módulos de conteúdo (CRUDs)

| Módulo | O que é | Observações |
| --- | --- | --- |
| **Pontos Turísticos** | Os lugares (praias, igrejas, fábricas...) | Upload de **capa, galeria e pictograma**; horários de funcionamento; flag "Apresentar em Maravilhas" + ordem |
| **Categorias** | Agrupam os pontos (ex.: Patrimônio & História, Natureza) | Categoria define se aparece nas 7 Maravilhas |
| **Eventos** | Agenda cultural | Data/hora, local, descrição; exportação `.ics` automática |
| **Slides** | Imagens do hero da home e da seção de história | Ordem define a sequência |
| **Estatísticas** | Números exibidos na seção "Nossa Cidade" | Ex.: "192 anos, 68k habitantes" |
| **Grupos Culturais** | Grupos (Reisado, Cacumbi...) | Aparecem em cultura/gastronomia |
| **Pratos Turísticos** | Pratos típicos | Com foto e descrição |
| **Tags Culturais** | Tags da cultura | Linkam para páginas de detalhe |
| **Conteúdos** | Textos do portal (título do hero, descrições das seções) | Chave → valor; use `\n` para quebrar linha |
| **Configurações** | Guia em PDF, vídeo institucional, título do site, SEO, contatos | O "Baixe o Guia" usa o arquivo configurado aqui |
| **Notícias** | Publicações do portal | Com publicação/destaque |
| **Roteiros** | Roteiros de visitação | Itens do roteiro hoje vêm do seed (edição via CMS no roadmap) |
| **Avaliações** | Avaliações deixadas no portal | **Moderação**: aprovar/reprovar antes de publicar |
| **Newsletter** | Lista de inscritos | **Exportar CSV**; ativar/inativar inscrito |
| **Usuários** | Contas do painel | Criar com perfil Gerenciador ou Operador |
| **Contatos** | Mensagens do formulário de contato | — |

**Padrões de uso:**
- Botões: o formulário tem botão primário **"Salvar"**; as listagens usam ações
  pequenas (editar/excluir/ativar).
- Campos **Ativo/Ordem** existem na maioria dos módulos — inative em vez de excluir
  quando o item tem vínculos.
- Excluir remove **em cascata** apenas o que é filho do item (ex.: mídias de um ponto).

### 3.3 Perfil Operador

O perfil **Operador** tem acesso restrito a **Eventos** e **Newsletter** — ideal para
uma equipe que mantém a agenda e a lista de e-mails sem mexer no resto do conteúdo.

---

## 4. Boas práticas de conteúdo

- **Imagens**: use fotos em orientação **paisagem** para capas (o portal corta com
  `object-fit: cover`). O **pictograma** deve ser um desenho/símbolo simples — ele
  aparece no preloader e nos cards.
- **Slide do hero**: a home usa os slides em sequência; mantenha pelo menos **3 slides**
  (as fotos polaroid da seção "Nossa Cidade" só aparecem com 3 ou mais).
- **Textos longos**: o campo "descrição" aceita parágrafos; use quebras de linha
  (Enter) — o portal converte em parágrafos.
- **SEO**: em Configurações, preencha `site-titulo`, `meta-descricao` e a imagem de
  compartilhamento — o portal gera `title`, `meta description`, **Open Graph** e
  **Twitter Cards** automaticamente para cada página.
- **Antes de publicar**: ative o item (Ativo = sim) — itens inativos não aparecem no
  portal, mas contam no dashboard.

---

## 5. Problemas comuns

| Sintoma | Causa provável | Solução |
| --- | --- | --- |
| Página do portal com erro 500 | Banco sem migrações ou slides com contagem incompatível | `dotnet ef database update`; manter 3+ slides |
| Login não funciona | Usuário/senha incorretos ou conta inativa | Redefina no Gerenciador (Usuários → excluir e recriar) |
| "Baixe o Guia" não aparece | Configuração `guia-pdf` sem arquivo | Configure o guia em **Configurações** |
| Imagem não carrega no portal | Arquivo removido/órfão | Reenvie a imagem no módulo correspondente |
| Analytics zerado | Navegação de teste com cookies bloqueados | Abrir em aba normal (não anônima) |
