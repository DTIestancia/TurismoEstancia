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
