// ===== Painel CMS — comportamentos globais =====

// Inicializa os ícones Lucide (o layout do painel carrega a lib com defer).
document.addEventListener('DOMContentLoaded', function () {
  if (window.lucide) {
    lucide.createIcons();
  }
});

// Confirmação de exclusão: qualquer form .js-confirm pede confirmação.
document.addEventListener('submit', function (e) {
  var form = e.target.closest && e.target.closest('form.js-confirm');
  if (!form) return;
  var msg = form.getAttribute('data-confirm') || 'Tem certeza que deseja excluir? Esta ação não pode ser desfeita.';
  if (!window.confirm(msg)) {
    e.preventDefault();
  }
});

// ===== Preview ao vivo de texto (Textos do portal) =====
// Renderiza o texto como o portal faz: quebras de linha viram <br> e tags
// conhecidas (<strong>, <em>, <br>) passam como HTML. O resto é escapado.
window.PrevisualizarTexto = function (textareaId) {
  var fonte = document.getElementById(textareaId);
  var alvo = document.getElementById('preview_' + textareaId);
  if (!fonte || !alvo) return;

  function atualizar() {
    var html = (fonte.value || '')
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/&lt;(\/?(?:strong|b|em|i|u|br)\s*\/?)&gt;/gi, '<$1>')
      .replace(/\n/g, '<br />');
    alvo.innerHTML = html || '<span class="painel-preview-vazio">O texto aparecerá aqui enquanto você digita…</span>';
  }

  fonte.addEventListener('input', atualizar);
  atualizar();
};

// ===== Prévia de ícone (Contatos) =====
// Mostra em tempo real o ícone (Lucide ou marca de rede social) que o rodapé
// vai exibir ao lado do contato. Os SVGs de marca são os MESMOS do portal
// (Views/Shared/Components/ContatosRodape/Default.cshtml) — mantidos em sincronia.
var ICONES_MARCA = {
  instagram: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect width="20" height="20" x="2" y="2" rx="5" ry="5"/><path d="M16 11.37A4 4 0 1 1 12.63 8 4 4 0 0 1 16 11.37z"/><line x1="17.5" x2="17.51" y1="6.5" y2="6.5"/></svg>',
  facebook: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M18 2h-3a5 5 0 0 0-5 5v3H7v4h3v8h4v-8h3l1-4h-4V7a1 1 0 0 1 1-1h3z"/></svg>',
  youtube: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M2.5 17a24.12 24.12 0 0 1 0-10 2 2 0 0 1 1.4-1.4 49.56 49.56 0 0 1 16.2 0A2 2 0 0 1 21.5 7a24.12 24.12 0 0 1 0 10 2 2 0 0 1-1.4 1.4 49.55 49.55 0 0 1-16.2 0A2 2 0 0 1 2.5 17"/><path d="m10 15 5-3-5-3z"/></svg>',
  whatsapp: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 11.5a8.38 8.38 0 0 1-.9 3.8 8.5 8.5 0 0 1-7.6 4.7 8.38 8.38 0 0 1-3.8-.9L3 21l1.9-5.7a8.38 8.38 0 0 1-.9-3.8 8.5 8.5 0 0 1 4.7-7.6 8.38 8.38 0 0 1 3.8-.9h.5a8.48 8.48 0 0 1 8 8v.5z"/></svg>',
  'message-circle': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 11.5a8.38 8.38 0 0 1-.9 3.8 8.5 8.5 0 0 1-7.6 4.7 8.38 8.38 0 0 1-3.8-.9L3 21l1.9-5.7a8.38 8.38 0 0 1-.9-3.8 8.5 8.5 0 0 1 4.7-7.6 8.38 8.38 0 0 1 3.8-.9h.5a8.48 8.48 0 0 1 8 8v.5z"/></svg>',
  'message-circle-more': '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 11.5a8.38 8.38 0 0 1-.9 3.8 8.5 8.5 0 0 1-7.6 4.7 8.38 8.38 0 0 1-3.8-.9L3 21l1.9-5.7a8.38 8.38 0 0 1-.9-3.8 8.5 8.5 0 0 1 4.7-7.6 8.38 8.38 0 0 1 3.8-.9h.5a8.48 8.48 0 0 1 8 8v.5z"/></svg>',
  tiktok: '<svg viewBox="0 0 24 24" fill="currentColor"><path d="M12.53.02C13.84 0 15.14.01 16.44 0c.08 1.53.63 3.09 1.75 4.17 1.12 1.11 2.7 1.62 4.24 1.79v4.03c-1.44-.05-2.89-.35-4.2-.97-.57-.26-1.1-.59-1.62-.93-.01 2.92.01 5.84-.02 8.75-.08 1.4-.54 2.79-1.35 3.94-1.31 1.92-3.58 3.17-5.91 3.21-1.43.08-2.86-.31-4.08-1.03-2.02-1.19-3.44-3.37-3.65-5.71-.02-.5-.03-1-.01-1.49.18-1.9 1.12-3.72 2.58-4.96 1.66-1.44 3.98-2.13 6.15-1.72.02 1.48-.04 2.96-.04 4.44-.99-.32-2.15-.23-3.02.37-.63.41-1.11 1.04-1.36 1.75-.21.51-.15 1.07-.14 1.61.24 1.64 1.82 3.02 3.5 2.87 1.12-.01 2.19-.66 2.77-1.61.19-.33.4-.67.41-1.06.1-1.79.06-3.57.07-5.36.01-4.03-.01-8.05.02-12.07z"/></svg>',
  x: '<svg viewBox="0 0 24 24" fill="currentColor"><path d="M18.901 1.153h3.68l-8.04 9.19L24 22.846h-7.406l-5.8-7.584-6.638 7.584H.474l8.6-9.83L0 1.154h7.594l5.243 6.932ZM17.61 20.644h2.039L6.486 3.24H4.298Z"/></svg>',
  twitter: '<svg viewBox="0 0 24 24" fill="currentColor"><path d="M18.901 1.153h3.68l-8.04 9.19L24 22.846h-7.406l-5.8-7.584-6.638 7.584H.474l8.6-9.83L0 1.154h7.594l5.243 6.932ZM17.61 20.644h2.039L6.486 3.24H4.298Z"/></svg>',
  linkedin: '<svg viewBox="0 0 24 24" fill="currentColor"><path d="M20.447 20.452h-3.554v-5.569c0-1.328-.027-3.037-1.852-3.037-1.853 0-2.136 1.445-2.136 2.939v5.667H9.351V9h3.414v1.561h.046c.477-.9 1.637-1.85 3.37-1.85 3.601 0 4.267 2.37 4.267 5.455v6.286zM5.337 7.433c-1.144 0-2.063-.926-2.063-2.065 0-1.138.92-2.063 2.063-2.063 1.14 0 2.064.925 2.064 2.063 0 1.139-.925 2.065-2.064 2.065zm1.782 13.019H3.555V9h3.564v11.452z"/></svg>'
};

// Sugestões por tipo de contato (índice do enum TipoContato: 0 Endereço, 1 Telefone, 2 Rede social, 3 E-mail).
var ICONES_SUGERIDOS = {
  '0': [
    { nome: 'map-pin' }, { nome: 'building-2' }, { nome: 'landmark' },
    { nome: 'navigation' }, { nome: 'home' }, { nome: 'school' }
  ],
  '1': [
    { nome: 'phone' }, { nome: 'whatsapp', brand: true }, { nome: 'ambulance' },
    { nome: 'car' }, { nome: 'siren' }, { nome: 'life-buoy' }, { nome: 'headset' }, { nome: 'send' }
  ],
  '2': [
    { nome: 'instagram', brand: true }, { nome: 'facebook', brand: true }, { nome: 'youtube', brand: true },
    { nome: 'whatsapp', brand: true }, { nome: 'tiktok', brand: true }, { nome: 'x', brand: true },
    { nome: 'linkedin', brand: true }, { nome: 'globe' }
  ],
  '3': [
    { nome: 'mail' }, { nome: 'at-sign' }, { nome: 'send' },
    { nome: 'inbox' }, { nome: 'mail-plus' }, { nome: 'headset' }
  ]
};

window.InicializarCampoIcone = function (inputId, previewId, chipsId, tipoId) {
  var input = document.getElementById(inputId);
  var preview = document.getElementById(previewId);
  var chips = document.getElementById(chipsId);
  var tipo = document.getElementById(tipoId);
  if (!input || !preview) return;

  function atualizarPreview() {
    var nome = (input.value || '').trim().toLowerCase();
    if (!nome) {
      preview.innerHTML = '<span class="painel-icone-preview-vazio" aria-hidden="true">?</span>';
      preview.title = '';
      return;
    }
    if (ICONES_MARCA[nome]) {
      preview.innerHTML = ICONES_MARCA[nome];
      preview.title = nome;
      return;
    }
    if (window.lucide) {
      // Mesmo mecanismo do portal: <i data-lucide> + createIcons() escopado ao preview.
      preview.innerHTML = '<i data-lucide="' + nome + '"></i>';
      lucide.createIcons({ nameAttr: 'data-lucide' }, preview);
      if (!preview.querySelector('i[data-lucide]')) {
        preview.title = nome;
        return;
      }
    }
    preview.innerHTML = '<span class="painel-icone-preview-invalido" aria-hidden="true">!</span>';
    preview.title = 'Ícone "' + nome + '" não encontrado. Use um nome Lucide ou uma rede social.';
  }

  function marcarAtivo() {
    if (!chips) return;
    var atual = (input.value || '').trim().toLowerCase();
    chips.querySelectorAll('.painel-icone-chip').forEach(function (c) {
      c.classList.toggle('active', c.getAttribute('data-icone') === atual);
    });
  }

  function montarChips() {
    if (!chips) return;
    chips.innerHTML = '';
    var lista = (tipo && ICONES_SUGERIDOS[tipo.value]) || ICONES_SUGERIDOS['1'];
    lista.forEach(function (icone) {
      var btn = document.createElement('button');
      btn.type = 'button';
      btn.className = 'painel-icone-chip';
      btn.setAttribute('data-icone', icone.nome);
      btn.title = icone.nome;
      if (icone.brand && ICONES_MARCA[icone.nome]) {
        btn.innerHTML = ICONES_MARCA[icone.nome];
      } else if (window.lucide) {
        btn.innerHTML = '<i data-lucide="' + icone.nome + '"></i>';
      } else {
        btn.textContent = icone.nome.charAt(0).toUpperCase();
      }
      btn.addEventListener('click', function () {
        input.value = icone.nome;
        atualizarPreview();
        marcarAtivo();
      });
      chips.appendChild(btn);
    });
    if (window.lucide) lucide.createIcons({ nameAttr: 'data-lucide' }, chips);
    marcarAtivo();
  }

  input.addEventListener('input', function () { atualizarPreview(); marcarAtivo(); });
  if (tipo) tipo.addEventListener('change', montarChips);
  montarChips();
  atualizarPreview();
};

// ===== Mapa interativo de posicionamento (Pontos turísticos) =====
// O usuário clica no mapa para posicionar o marcador; as porcentagens X/Y
// (LeftPercent/TopPercent) são calculadas a partir do clique e mantidas em
// sincronia com os campos numéricos (para ajuste fino).
window.InicializarMapaPonto = function (opts) {
  var mapa = document.getElementById(opts.mapaId);
  var marcador = document.getElementById(opts.marcadorId);
  var vazio = opts.vazioId ? document.getElementById(opts.vazioId) : null;
  var left = document.getElementById(opts.leftId);
  var top = document.getElementById(opts.topId);
  if (!mapa || !left || !top) return;

  function posicionar(xPct, yPct) {
    xPct = Math.max(0, Math.min(100, xPct));
    yPct = Math.max(0, Math.min(100, yPct));
    left.value = Math.round(xPct);
    top.value = Math.round(yPct);
    if (marcador) {
      marcador.hidden = false;
      marcador.style.left = xPct + '%';
      marcador.style.top = yPct + '%';
    }
    if (vazio) vazio.style.display = 'none';
  }

  mapa.addEventListener('click', function (e) {
    var rect = mapa.getBoundingClientRect();
    var xPct = ((e.clientX - rect.left) / rect.width) * 100;
    var yPct = ((e.clientY - rect.top) / rect.height) * 100;
    posicionar(xPct, yPct);
  });

  // Sincroniza o marcador quando os campos numéricos mudam (ajuste fino).
  function lerInputs() {
    var x = parseFloat(left.value);
    var y = parseFloat(top.value);
    if (!isNaN(x) && !isNaN(y)) posicionar(x, y);
  }

  left.addEventListener('input', lerInputs);
  top.addEventListener('input', lerInputs);
  lerInputs();
};

// ===== Modal de cadastro (telas de lista → formulário embutido) =====
// Botões .js-abrir-criar (data-url) abrem o dialog com o formulário de Criar
// num iframe (?embutido=1, sem sidebar/topbar). Ao salvar, o iframe avisa via
// postMessage e o modal recarrega a página para a lista mostrar o item novo.
document.addEventListener('click', function (e) {
  var botao = e.target.closest && e.target.closest('.js-abrir-criar');
  if (!botao) return;

  var url = botao.getAttribute('data-url');
  if (!url) return;

  var dialog = document.getElementById('criarDialog');
  var frame = document.getElementById('criarDialogFrame');
  var titulo = document.getElementById('criarDialogTitulo');
  var aba = document.getElementById('criarDialogAbrirAba');
  if (!dialog || !frame) return;

  var urlEmbutida = url.indexOf('?') >= 0 ? url + '&embutido=1' : url + '?embutido=1';
  if (titulo) titulo.textContent = botao.getAttribute('data-titulo') || 'Cadastrar';
  if (aba) aba.setAttribute('href', url);
  frame.setAttribute('src', urlEmbutida);
  if (typeof dialog.showModal === 'function') dialog.showModal();
  else dialog.setAttribute('open', '');
  e.preventDefault();
});

// Fecha um dialog com a animação de saída (fade + escala): adiciona a classe
// .dialog-fechando, espera a animação e só então chama close().
window.FecharDialogPainel = function (id) {
  var dialog = document.getElementById(id);
  if (!dialog || !dialog.open) return;

  var frame = dialog.querySelector('iframe');
  if (frame) frame.setAttribute('src', 'about:blank');

  dialog.classList.add('dialog-fechando');
  setTimeout(function () {
    if (typeof dialog.close === 'function') dialog.close();
    else dialog.removeAttribute('open');
    dialog.classList.remove('dialog-fechando');
  }, 180);
};

// Botão fechar + clique no backdrop + Esc (criarDialog).
document.addEventListener('click', function (e) {
  var fechar = e.target.closest && e.target.closest('#criarDialogFechar');
  if (fechar) {
    window.FecharDialogPainel('criarDialog');
    return;
  }
  var alvo = e.target;
  if (alvo.id === 'criarDialog') window.FecharDialogPainel('criarDialog'); // clique no backdrop
});

document.addEventListener('keydown', function (e) {
  if (e.key === 'Escape') {
    var dialog = document.getElementById('criarDialog');
    if (dialog && dialog.open) window.FecharDialogPainel('criarDialog');
  }
});

// Depois de salvar dentro do iframe, recarrega a página para a lista atualizar.
window.addEventListener('message', function (e) {
  if (e.data === 'painel-salvo') {
    fecharCriarDialog();
    window.location.reload();
  }
});

// ===== Newsletter: busca ao vivo + dialog de disparo =====
// (elementos existem apenas na página de newsletter do painel)

// Busca: filtra as linhas da tabela por e-mail/origem, mostrando o contador.
var newsletterSearch = document.getElementById('newsletterSearch');
var newsletterEmpty = document.getElementById('newsletterEmpty');
var newsletterCount = document.getElementById('newsletterCount');

function atualizarBuscaNewsletter() {
  if (!newsletterSearch || !newsletterCount) return;
  var linhas = document.querySelectorAll('#newsletterTable tbody tr');
  var termo = newsletterSearch.value.trim().toLowerCase();
  var visiveis = 0;
  var total = linhas.length;

  linhas.forEach(function (linha) {
    var mostrar = !termo || (linha.getAttribute('data-search') || '').indexOf(termo) !== -1;
    linha.style.display = mostrar ? '' : 'none';
    if (mostrar) visiveis++;
  });

  newsletterCount.textContent = termo
    ? visiveis + ' de ' + total + ' inscrição' + (total === 1 ? '' : 'ões')
    : total + ' inscrição' + (total === 1 ? '' : 'ões');

  if (newsletterEmpty) {
    if (total === 0) {
      newsletterEmpty.textContent = 'Nenhuma inscrição recebida ainda.';
      newsletterEmpty.style.display = '';
    } else if (visiveis === 0) {
      newsletterEmpty.textContent = 'Nenhuma inscrição encontrada para a busca.';
      newsletterEmpty.style.display = '';
    } else {
      newsletterEmpty.style.display = 'none';
    }
  }
}

if (newsletterSearch) {
  newsletterSearch.addEventListener('input', atualizarBuscaNewsletter);
  atualizarBuscaNewsletter();
}

// Dialog de disparo: abre via data-dialog-open, fecha via data-dialog-close,
// backdrop ou Esc (nativo do <dialog>).
document.addEventListener('click', function (e) {
  var abrir = e.target.closest && e.target.closest('[data-dialog-open]');
  if (abrir) {
    var dialog = document.getElementById(abrir.getAttribute('data-dialog-open'));
    if (dialog && typeof dialog.showModal === 'function') {
      dialog.showModal();
      e.preventDefault();
    }
    return;
  }

  var fechar = e.target.closest && e.target.closest('[data-dialog-close]');
  if (fechar) {
    var dlg = fechar.closest('dialog');
    if (dlg && typeof dlg.close === 'function') dlg.close();
    return;
  }

  // Fecha ao clicar no fundo (fora do painel interno).
  var alvo = e.target;
  if (alvo.tagName === 'DIALOG') {
    var box = alvo.querySelector('.painel-dialog-inner');
    if (!box || !box.contains(e.target)) alvo.close();
  }
});
