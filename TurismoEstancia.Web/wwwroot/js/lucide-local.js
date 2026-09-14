// ============================================================
// lucide-local.js — os ícones do sistema sem a biblioteca de 414 KB
//
// O portal e o painel carregavam o pacote UMD completo do Lucide pelo jsdelivr
// (414 KB por página) só para desenhar ~150 ícones. Aqui a mesma chamada
// `window.lucide.createIcons()` desenha a partir do sprite local
// wwwroot/img/icones.svg, gerado por tools/gerar-sprite-icones.py.
//
// Contrato mantido em relação ao lucide da CDN:
//   * o <i data-lucide> vira <svg data-lucide> COPIANDO os atributos do elemento
//     original — é por isso que o CSS `[data-lucide] { width: 1em }` continua
//     valendo depois da troca, e que `width="22"` no markup é respeitado;
//   * aceita `{ nameAttr: 'data-lucide' }` e um segundo argumento de escopo
//     (elemento, NodeList ou seletor) — o painel usa isso para atualizar só a
//     prévia do ícone;
//   * nome desconhecido NÃO é substituído (o painel depende dessa ausência para
//     descobrir e esconder ícones que não existem) e sai aviso no console.
//
// A lista de nomes conhecidos é gerada — não edite à mão.
// ============================================================
(function () {
  'use strict';

  // >>> gerado por tools/gerar-sprite-icones.py — não edite à mão
  var ICONES = [
  'accessibility',
  'alert-circle',
  'alert-triangle',
  'ambulance',
  'anchor',
  'arrow-left',
  'arrow-right',
  'at-sign',
  'bar-chart-3',
  'bed-double',
  'bed-single',
  'beer',
  'bell',
  'bike',
  'bird',
  'book-open',
  'briefcase',
  'building',
  'building-2',
  'bus',
  'calendar',
  'calendar-clock',
  'calendar-days',
  'calendar-plus',
  'camera',
  'car',
  'castle',
  'check',
  'check-circle',
  'chevron-down',
  'chevron-left',
  'chevron-right',
  'chevron-up',
  'church',
  'clock',
  'coffee',
  'compass',
  'contact',
  'copy',
  'credit-card',
  'crown',
  'database',
  'download',
  'drama',
  'droplet',
  'expand',
  'external-link',
  'eye',
  'eye-off',
  'file-code',
  'file-down',
  'file-text',
  'film',
  'fish',
  'flag',
  'flame',
  'folder-open',
  'folder-tree',
  'fuel',
  'gem',
  'globe',
  'guitar',
  'headset',
  'heart',
  'help-circle',
  'history',
  'home',
  'hospital',
  'hotel',
  'image',
  'image-plus',
  'images',
  'inbox',
  'info',
  'landmark',
  'layout-dashboard',
  'leaf',
  'life-buoy',
  'link',
  'list',
  'log-in',
  'log-out',
  'mail',
  'mail-check',
  'mail-plus',
  'map',
  'map-pin',
  'megaphone',
  'message-circle',
  'message-circle-more',
  'message-square-heart',
  'mic',
  'mountain',
  'mountain-snow',
  'mouse-pointer-click',
  'music',
  'navigation',
  'newspaper',
  'palette',
  'party-popper',
  'pencil',
  'phone',
  'pizza',
  'plane',
  'plus',
  'plus-circle',
  'rotate-ccw',
  'route',
  'sailboat',
  'save',
  'school',
  'search',
  'send',
  'settings',
  'share-2',
  'shell',
  'shield',
  'shield-alert',
  'ship',
  'shopping-bag',
  'siren',
  'smartphone',
  'sparkles',
  'star',
  'store',
  'sun',
  'tag',
  'tags',
  'tent',
  'theater',
  'ticket',
  'train',
  'trash-2',
  'tree-pine',
  'trees',
  'trending-up',
  'trophy',
  'type',
  'upload',
  'user-check',
  'user-x',
  'users',
  'utensils',
  'utensils-crossed',
  'video',
  'waves',
  'wifi',
  'wine',
  'x',
  'zap',
  ];
  // <<< fim do bloco gerado

  var SVG_NS = 'http://www.w3.org/2000/svg';
  var XLINK_NS = 'http://www.w3.org/1999/xlink';

  // Caminho do sprite derivado do próprio <script>, carregando junto o ?v= que o
  // asp-append-version coloca na tag: funciona em publicação por subpasta e o
  // sprite herda o mesmo cache longo. O gerador escreve a lista acima e o sprite
  // no mesmo passo, então o ?v= muda sempre que os ícones mudam.
  var SPRITE = (function () {
    var atual = document.currentScript;
    if (atual && atual.src) {
      var versao = (atual.src.match(/\?v=([^&]+)/) || [])[1];
      var caminho = atual.src.replace(/\/js\/lucide-local\.js.*$/, '/img/icones.svg');
      return versao ? caminho + '?v=' + versao : caminho;
    }
    var base = (document.body && document.body.getAttribute('data-app-base')) || '';
    return base.replace(/\/?$/, '/') + 'img/icones.svg';
  })();

  function normalizar(nome) {
    return String(nome == null ? '' : nome).trim().toLowerCase().replace(/[\s_]+/g, '-');
  }

  function criarSvg(nome, origem) {
    var svg = document.createElementNS(SVG_NS, 'svg');
    var i, atributo;

    for (i = 0; i < origem.attributes.length; i++) {
      atributo = origem.attributes[i];
      if (atributo.name !== 'class' && atributo.name !== 'data-lucide') {
        svg.setAttribute(atributo.name, atributo.value);
      }
    }

    svg.setAttribute('data-lucide', nome);
    if (!svg.hasAttribute('viewBox')) svg.setAttribute('viewBox', '0 0 24 24');
    svg.setAttribute('fill', 'none');
    svg.setAttribute('stroke', 'currentColor');
    svg.setAttribute('stroke-width', '2');
    svg.setAttribute('stroke-linecap', 'round');
    svg.setAttribute('stroke-linejoin', 'round');
    if (!svg.hasAttribute('width')) svg.setAttribute('width', '24');
    if (!svg.hasAttribute('height')) svg.setAttribute('height', '24');
    if (!svg.hasAttribute('role')) {
      svg.setAttribute('aria-hidden', 'true'); // decorativo: o rótulo é o texto ao lado
    }

    var classes = 'lucide lucide-' + nome;
    if (origem.getAttribute('class')) classes += ' ' + origem.getAttribute('class');
    svg.setAttribute('class', classes);

    var uso = document.createElementNS(SVG_NS, 'use');
    uso.setAttribute('href', SPRITE + '#' + nome);
    uso.setAttributeNS(XLINK_NS, 'xlink:href', SPRITE + '#' + nome);
    svg.appendChild(uso);
    return svg;
  }

  function raizes(escopo) {
    if (!escopo) return [document];
    if (typeof escopo === 'string') {
      return Array.prototype.slice.call(document.querySelectorAll(escopo));
    }
    if (escopo.nodeType === undefined && escopo.length !== undefined) {
      return Array.prototype.slice.call(escopo); // NodeList ou array
    }
    return [escopo];
  }

  function createIcons(opcoes, escopo) {
    var nameAttr = (opcoes && opcoes.nameAttr) || 'data-lucide';
    var seletor = '[' + nameAttr + ']';
    var alvos = [];

    raizes(escopo).forEach(function (raiz) {
      if (!raiz || !raiz.querySelectorAll) return;
      if (raiz.nodeType === 1 && raiz.matches && raiz.matches(seletor)) alvos.push(raiz);
      Array.prototype.forEach.call(raiz.querySelectorAll(seletor), function (el) { alvos.push(el); });
    });

    var trocados = 0;
    alvos.forEach(function (el) {
      var nome = normalizar(el.getAttribute(nameAttr));
      if (ICONES.indexOf(nome) === -1) {
        if (nome && window.console) {
          console.warn('[ícones] nome fora do sprite local:', nome, el.outerHTML);
        }
        return;
      }
      if (el.parentNode) {
        el.parentNode.replaceChild(criarSvg(nome, el), el);
        trocados++;
      }
    });

    return trocados;
  }

  window.lucide = {
    createIcons: createIcons,
    icones: ICONES,
    sprite: SPRITE
  };
})();
