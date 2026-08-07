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
