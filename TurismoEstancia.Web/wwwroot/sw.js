// Service Worker do portal (v1): shell offline-first para estáticos e
// miniaturas (imutáveis), network-first para navegação com fallback à home.
// NUNCA intercepta: Gerenciador/Operador, API POST/beacon, Range (vídeo).
var VERSAO = 'turismo-v1';
var ESTATICOS = ["/", "/css/main.css", "/js/portal.js", "/favicon.svg"];

self.addEventListener('install', function (e) {
  e.waitUntil(
    caches.open(VERSAO).then(function (cache) {
      return Promise.all(ESTATICOS.map(function (url) {
        return cache.add(url).catch(function () { /* offline na instalação: ignora */ });
      });
    }).then(function () { return self.skipWaiting(); })
  );
});

self.addEventListener('activate', function (e) {
  e.waitUntil(
    caches.keys().then(function (chaves) {
      return Promise.all(chaves
        .filter(function (c) { return c !== VERSAO; })
        .map(function (c) { return caches.delete(c); }));
    }).then(function () { return self.clients.claim(); })
  );
});

function eAdmin(url) {
  return url.pathname.indexOf('/Gerenciador') === 0 || url.pathname.indexOf('/Operador') === 0;
}

self.addEventListener('fetch', function (e) {
  var req = e.request;
  if (req.method !== 'GET') return;
  var url = new URL(req.url);
  if (url.origin !== self.location.origin) return;
  if (eAdmin(url)) return;
  if (req.headers.has('range')) return; // streaming de vídeo direto na rede

  var ehNavegacao = req.mode === 'navigate' ||
    (req.headers.get('accept') || '').indexOf('text/html') !== -1;

  if (ehNavegacao) {
    // Página sempre fresca; sem rede, a última home salva.
    e.respondWith(
      fetch(req).then(function (resp) {
        // Só a home vira fallback offline (outras páginas não se passam por ela).
        if (url.pathname === '/') {
          var copia = resp.clone();
          caches.open(VERSAO).then(function (cache) { cache.put('/', copia); });
        }
        return resp;
      }).catch(function () {
        return caches.match('/').then(function (r) { return r || Response.error(); });
      })
    );
    return;
  }

  var cacheavel = url.pathname.indexOf('/arquivo/') === 0 ||
    url.pathname.indexOf('/css/') === 0 ||
    url.pathname.indexOf('/js/') === 0 ||
    url.pathname.indexOf('/img/') === 0 ||
    url.pathname.indexOf('/fonts/') === 0;

  if (cacheavel) {
    // Stale-while-revalidate: responde do cache e atualiza em segundo plano.
    e.respondWith(
      caches.match(req).then(function (hit) {
        var busca = fetch(req).then(function (resp) {
          if (resp && resp.ok) {
            var copia = resp.clone();
            caches.open(VERSAO).then(function (cache) { cache.put(req, copia); });
          }
          return resp;
        }).catch(function () { return hit; });
        return hit || busca;
      })
    );
  }
});
