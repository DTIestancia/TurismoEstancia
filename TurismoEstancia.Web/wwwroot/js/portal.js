// ============================================================
// Portal "Descubra Estância" — JS portado do protótipo (Fase 5)
// Adaptações: dados do mapa vêm de window.turismoEstancia.mapa,
// newsletter e avaliação usam POST reais, modal mostra avaliações.
// ============================================================
document.addEventListener('DOMContentLoaded', function () {

  // ===== Page Preloader =====
  (function hidePageLoader() {
    var loader = document.getElementById('pageLoader');
    if (!loader) return;

    function completeBar() {
      loader.classList.add('completing');
      setTimeout(function () {
        loader.classList.add('hidden');
        setTimeout(function () {
          if (loader.parentNode) loader.parentNode.removeChild(loader);
        }, 800);
      }, 400);
    }

    if (document.readyState === 'complete') completeBar();
    else {
      window.addEventListener('load', completeBar);
      setTimeout(completeBar, 5000);
    }
  })();

  // ===== Dark Mode Toggle =====
  (function initTheme() {
    var themeToggle = document.getElementById('themeToggle');
    var html = document.documentElement;
    if (!themeToggle) return;

    var savedTheme = localStorage.getItem('theme');
    if (savedTheme === 'dark' || (!savedTheme && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
      html.setAttribute('data-theme', 'dark');
    }

    themeToggle.addEventListener('click', function () {
      var isDark = html.getAttribute('data-theme') === 'dark';
      if (isDark) { html.removeAttribute('data-theme'); localStorage.setItem('theme', 'light'); }
      else { html.setAttribute('data-theme', 'dark'); localStorage.setItem('theme', 'dark'); }
      var liveIcon = document.getElementById('themeToggleIcon');
      if (liveIcon) liveIcon.setAttribute('data-lucide', isDark ? 'moon' : 'sun');
      if (typeof lucide !== 'undefined') lucide.createIcons();
    });
  })();

  if (typeof lucide !== 'undefined') lucide.createIcons();

  // ===== Hero Background Video (autoplay mudo, em loop) =====
  // O vídeo do hero é configurado no Gerenciador (video-institucional). Toca
  // sozinho, mudo e em loop; um load()+play() explícito destrava o Safari iOS.
  (function initHeroVideo() {
    var video = document.getElementById('heroBgVideo');
    if (!video) return;

    function tentarPlay() {
      var p = video.play();
      if (p && typeof p.catch === 'function') {
        p.catch(function () {
          video.muted = true;
          video.play().catch(function () {});
        });
      }
    }

    video.addEventListener('canplay', tentarPlay);
    window.addEventListener('load', tentarPlay);
    document.addEventListener('visibilitychange', function () {
      if (document.hidden) video.pause();
      else tentarPlay();
    });
  })();

  // ===== Seletor de idioma do site (UI — guarda a preferência) =====
  (function initSiteLang() {
    var sel = document.getElementById('siteLang');
    if (!sel) return;
    var salvo = localStorage.getItem('site-lang');
    if (salvo) sel.value = salvo;
    if (salvo && document.documentElement.lang !== salvo) document.documentElement.lang = salvo || 'pt-BR';
    sel.addEventListener('change', function () {
      var lang = sel.value || 'pt-BR';
      localStorage.setItem('site-lang', lang);
      document.documentElement.lang = lang;
    });
  })();

  // ===== Explorador "Conheça Estância" (carrossel em baralho, estilo print) =====
  // Cada aba (História, Cultura, Gastronomia, Experiências) alimenta um "deck"
  // de cartas com foto de fundo. A carta ativa ocupa o centro com título,
  // descrição e botão; as próximas espreitam à direita. As setas giram o baralho.
  (function initConhecaEstancia() {
    var dataEl = document.getElementById('conhecaData');
    var slide = document.getElementById('conhecaSlide');
    if (!dataEl || !slide) return;
    var tabs;
    try { tabs = JSON.parse(dataEl.textContent); } catch (e) { return; }
    if (!tabs || !tabs.length) return;

    var prevBtn = document.getElementById('conhecaPrev');
    var nextBtn = document.getElementById('conhecaNext');
    var tabButtons = document.querySelectorAll('.conheca-tab');
    var activeTabIdx = 0;

    function buildCard(item, rotulo) {
      var el = document.createElement('div');
      el.className = 'conheca-card-item';
      if (item.imagem) el.style.backgroundImage = "url('" + item.imagem + "')";
      else el.classList.add('no-photo');
      el.dataset.nome = item.nome || '';

      var content = document.createElement('div');
      content.className = 'conheca-card-content';
      content.innerHTML =
        '<span class="conheca-card-pill">' + esc(rotulo) + '</span>' +
        '<h3 class="conheca-card-name">' + esc(item.nome) + '</h3>' +
        '<p class="conheca-card-des">' + esc(item.descricao || '') + '</p>' +
        '<a class="conheca-card-link" href="' + esc(item.url || '#') + '">Ver detalhes <span aria-hidden="true">→</span></a>';
      el.appendChild(content);
      return el;
    }

    function renderDeck() {
      slide.innerHTML = '';
      var tab = tabs[activeTabIdx];
      var itens = tab.itens || [];

      if (!itens.length) {
        var empty = document.createElement('div');
        empty.className = 'conheca-empty';
        empty.textContent = 'Em breve, novidades sobre ' + tab.rotulo + '.';
        slide.appendChild(empty);
        return;
      }

      // Baralho circular: começa pela última carta para a 1ª ficar em destaque.
      var fila = itens.slice();
      var ultima = fila.pop();
      if (ultima) slide.appendChild(buildCard(ultima, tab.rotulo));
      fila.forEach(function (it) { slide.appendChild(buildCard(it, tab.rotulo)); });
    }

    // Rotação: move a 1ª carta para o fim (next) ou a última para o início (prev).
    function moverPrimeira() {
      var first = slide.querySelector('.conheca-card-item');
      if (first) slide.appendChild(first);
    }
    function moverUltima() {
      var cards = slide.querySelectorAll('.conheca-card-item');
      var last = cards[cards.length - 1];
      if (last) slide.insertBefore(last, slide.firstChild);
    }

    tabButtons.forEach(function (btn) {
      btn.addEventListener('click', function () {
        var chave = this.getAttribute('data-tab');
        var idx = -1;
        tabs.forEach(function (t, i) { if (t.chave === chave) idx = i; });
        if (idx < 0) return;
        activeTabIdx = idx;
        tabButtons.forEach(function (b) { b.classList.toggle('active', b === btn); });
        renderDeck();
      });
    });

    if (nextBtn) nextBtn.addEventListener('click', moverPrimeira);
    if (prevBtn) prevBtn.addEventListener('click', moverUltima);

    // Abre a primeira aba que tenha conteúdo.
    var idxInicial = 0;
    for (var i = 0; i < tabs.length; i++) {
      if ((tabs[i].itens || []).length) { idxInicial = i; break; }
    }
    activeTabIdx = idxInicial;
    tabButtons.forEach(function (b, i) { b.classList.toggle('active', i === idxInicial); });
    renderDeck();
  })();

  // ===== Reading Progress Bar =====
  (function initProgressBar() {
    var bar = document.getElementById('progressBar');
    if (!bar) return;
    var ticking = false;
    function updateProgress() {
      var scrollTop = window.scrollY || window.pageYOffset;
      var docHeight = document.documentElement.scrollHeight - window.innerHeight;
      var pct = docHeight > 0 ? (scrollTop / docHeight) * 100 : 0;
      bar.style.width = Math.min(pct, 100) + '%';
      ticking = false;
    }
    window.addEventListener('scroll', function () {
      if (!ticking) { requestAnimationFrame(updateProgress); ticking = true; }
    }, { passive: true });
    updateProgress();
  })();

  // ===== Botão voltar ao topo =====
  (function initScrollTop() {
    const btn = document.getElementById('scrollTop');
    if (!btn) return;
    let tickingTop = false;
    function updateScrollTop() {
      const y = window.scrollY || window.pageYOffset;
      btn.classList.toggle('visible', y > 480);
      tickingTop = false;
    }
    window.addEventListener('scroll', function () {
      if (!tickingTop) { requestAnimationFrame(updateScrollTop); tickingTop = true; }
    }, { passive: true });
    btn.addEventListener('click', function () {
      window.scrollTo({ top: 0, behavior: 'smooth' });
    });
    updateScrollTop();
  })();

  // ===== Feedback da newsletter: rola até a mensagem após o redirect =====
  (function scrollToNewsletterFeedback() {
    const feedback = document.querySelector('.newsletter-feedback');
    if (!feedback) return;
    // Espera o preloader sumir (window.load + transição de ~450ms) para o
    // visitante ver a confirmação ao chegar no rodapé.
    setTimeout(function () {
      feedback.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }, 900);
  })();

  // ===== Navbar fixa sempre visível + fundo ao rolar =====
  const navbar = document.getElementById('navbar');
  let tickingNav = false;

  function updateNavbar() {
    if (!navbar) return;
    const currentScrollY = window.scrollY;
    if (currentScrollY > 50) navbar.classList.add('scrolled');
    else navbar.classList.remove('scrolled');
    tickingNav = false;
  }
  window.addEventListener('scroll', function () {
    if (!tickingNav) { requestAnimationFrame(updateNavbar); tickingNav = true; }
  }, { passive: true });

  // Menu mobile genérico: vale para a navbar da home (breakpoint 1024px) e
  // para o header das páginas internas (breakpoint 860px).
  function initMobileMenu(navToggle, navLinks, breakpoint) {
    if (!navToggle || !navLinks) return;
    let menuOpen = false;

    function toggleMenu(open) {
      const isOpen = open !== undefined ? open : !menuOpen;
      navLinks.classList.toggle('open', isOpen);
      navToggle.classList.toggle('open', isOpen);
      document.body.style.overflow = isOpen ? 'hidden' : '';
      navToggle.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
      navToggle.setAttribute('aria-label', isOpen ? 'Fechar menu' : 'Abrir menu');
      menuOpen = isOpen;
    }

    navToggle.addEventListener('click', function () { toggleMenu(); });
    navLinks.querySelectorAll('a').forEach(function (link) {
      link.addEventListener('click', function () { toggleMenu(false); });
    });
    // Clicar fora do menu (overlay ou qualquer lugar da página) fecha.
    document.addEventListener('click', function (e) {
      if (!menuOpen) return;
      if (navLinks.contains(e.target)) return;
      if (navToggle.contains(e.target)) return;
      toggleMenu(false);
    });
    document.addEventListener('keydown', function (e) {
      if (e.key === 'Escape' && menuOpen) toggleMenu(false);
    });
    window.addEventListener('resize', function () {
      if (window.innerWidth > breakpoint && menuOpen) toggleMenu(false);
    });
  }

  initMobileMenu(document.getElementById('navToggle'), document.getElementById('navLinks'), 1024);
  initMobileMenu(document.getElementById('paginasNavToggle'), document.getElementById('paginasNavLinks'), 860);

  // ===== Wonder/POI Modal =====
  const modal = document.getElementById('wonderModal');
  const modalImg = document.getElementById('modalImg');
  const modalIcon = document.getElementById('modalIcon');
  const modalIconWrap = document.getElementById('modalIconWrap');
  const modalCategory = document.getElementById('modalCategory');
  const modalTitle = document.getElementById('modalTitle');
  const modalDesc = document.getElementById('modalDesc');
  const modalDetail = document.getElementById('modalDetail');
  const modalTag = document.getElementById('modalTag');
  const modalClose = document.getElementById('modalClose');
  const modalAddress = document.getElementById('modalAddress');
  const modalAddressText = document.getElementById('modalAddressText');
  const modalDirections = document.getElementById('modalDirections');
  const modalDirectionsText = document.getElementById('modalDirectionsText');

  // Bloco de avaliações
  const modalAvaliacoes = document.getElementById('modalAvaliacoes');
  const modalAvaliacoesList = document.getElementById('modalAvaliacoesList');
  const avaliacaoPontoId = document.getElementById('avaliacaoPontoId');

  function renderAvaliacoes(lista) {
    if (!modalAvaliacoes || !modalAvaliacoesList) return;
    modalAvaliacoesList.innerHTML = '';
    if (!lista || lista.length === 0) {
      modalAvaliacoes.hidden = true;
      return;
    }
    modalAvaliacoes.hidden = false;
    lista.forEach(function (av) {
      var item = document.createElement('div');
      item.className = 'modal-avaliacao-item';
      var estrelas = '★'.repeat(av.nota) + '☆'.repeat(5 - av.nota);
      item.innerHTML =
        '<div class="modal-avaliacao-head">' +
        '<strong>' + esc(av.nome) + '</strong>' +
        '<span class="modal-avaliacao-stars">' + estrelas + '</span></div>' +
        (av.comentario ? '<p class="modal-avaliacao-comentario">' + esc(av.comentario) + '</p>' : '');
      modalAvaliacoesList.appendChild(item);
    });
  }

  function carregarAvaliacoes(pontoId) {
    if (!avaliacaoPontoId) return;
    avaliacaoPontoId.value = pontoId;
    if (!modalAvaliacoes) return;
    fetch(appBase() + 'Avaliacao/ListarPorPonto/' + pontoId)
      .then(function (r) { return r.json(); })
      .then(renderAvaliacoes)
      .catch(function () { if (modalAvaliacoes) modalAvaliacoes.hidden = true; });
  }

  function setIconWrap(categoriaChave) {
    if (!modalIconWrap) return;
    modalIconWrap.className = 'modal-icon-wrap';
    var cls = String(categoriaChave || '').toLowerCase();
    if (cls === 'nature') modalIconWrap.classList.add('nature');
    else if (cls === 'hotel') modalIconWrap.classList.add('hotel');
    else if (cls === 'food') modalIconWrap.classList.add('food');
    else if (cls === 'service') modalIconWrap.classList.add('service');
    else modalIconWrap.classList.add('heritage');
  }

  function openWonderModal(card) {
    modalAddress.classList.remove('show');
    modalDirections.classList.remove('show');
    modalIcon.setAttribute('data-lucide', card.dataset.wonderIcon || 'star');
    modalCategory.textContent = card.dataset.wonderCategory || '';
    modalTitle.textContent = card.dataset.wonderTitle || '';
    modalDesc.textContent = card.dataset.wonderDesc || '';
    modalDetail.textContent = card.dataset.wonderDetail || '';
    modalTag.textContent = card.dataset.wonderTag || '';

    var img = card.dataset.wonderImg || '';
    modalImg.src = img;
    modalImg.alt = card.dataset.wonderTitle || '';
    modalImg.classList.toggle('hidden', !img);

    setIconWrap(card.dataset.wonderCategoriaChave);
    if (typeof lucide !== 'undefined') lucide.createIcons();
    modal.classList.add('active');
    document.body.style.overflow = 'hidden';

    // Ponto turístico (para avaliações) — via data-wonder-point-id
    var pontoId = card.dataset.wonderPointId;
    if (pontoId) carregarAvaliacoes(pontoId);

    // Botão "Ver no mapa"
    var oldBtn = document.getElementById('mapViewBtn');
    if (oldBtn) oldBtn.remove();
    var viewBtn = document.createElement('button');
    viewBtn.id = 'mapViewBtn';
    viewBtn.className = 'custom-popup-btn';
    viewBtn.innerHTML = '<i data-lucide="map-pin" class="icon-lg"></i> Ver no mapa';
    viewBtn.addEventListener('click', function () {
      closeWonderModal();
      var mapSection = document.querySelector('.section-map');
      if (mapSection) mapSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
      setTimeout(function () {
        if (typeof window.flyToWonder === 'function') window.flyToWonder(card.dataset.wonderTitle);
      }, 600);
    });
    if (modalDetail) modalDetail.parentNode.insertBefore(viewBtn, modalDetail.nextSibling);
    if (typeof lucide !== 'undefined') lucide.createIcons();
  }

  function closeWonderModal() {
    modal.classList.remove('active');
    document.body.style.overflow = '';
  }

  document.querySelectorAll('.wonder-card').forEach(function (card) {
    card.addEventListener('click', function () { openWonderModal(card); });
    card.addEventListener('keydown', function (e) {
      if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); openWonderModal(card); }
    });
  });

  if (modalClose) modalClose.addEventListener('click', closeWonderModal);
  if (modal) {
    modal.addEventListener('click', function (e) { if (e.target === modal) closeWonderModal(); });
  }
  document.addEventListener('keydown', function (e) {
    // O modal só existe na home — páginas internas não têm #wonderModal.
    if (e.key === 'Escape' && modal && modal.classList.contains('active')) closeWonderModal();
  });

  // ===== Avaliação: seletor de notas =====
  document.querySelectorAll('#avaliacaoNotas .avaliacao-nota').forEach(function (btn) {
    btn.addEventListener('click', function () {
      var nota = parseInt(this.getAttribute('data-nota'), 10);
      document.getElementById('avaliacaoNotaInput').value = nota;
      document.querySelectorAll('#avaliacaoNotas .avaliacao-nota').forEach(function (b) {
        b.classList.toggle('active', parseInt(b.getAttribute('data-nota'), 10) <= nota);
      });
    });
  });

  // ===== Newsletter (POST real com anti-forgery) =====
  var newsletterForm = document.getElementById('newsletterForm');
  if (newsletterForm) {
    newsletterForm.addEventListener('submit', function (e) {
      e.preventDefault();
      var email = newsletterForm.querySelector('input[name="email"]').value.trim();
      var consentimento = newsletterForm.querySelector('input[name="consentimentoLgpd"]').checked;
      var token = newsletterForm.querySelector('input[name="__RequestVerificationToken"]').value;
      if (!consentimento) { alert('É necessário consentir com a LGPD para receber a newsletter.'); return; }

      fetch(appBase() + 'Newsletter/Inscrever', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8',
          'RequestVerificationToken': token
        },
        body: 'email=' + encodeURIComponent(email) + '&consentimentoLgpd=true&__RequestVerificationToken=' + encodeURIComponent(token)
      }).then(function (r) {
        if (r.redirected) { window.location.href = r.url; return; }
        window.location.reload();
      });
    });
  }

  // ===== Interactive Custom Illustrated Map (Data-Driven) =====
  function initEstanciaMap() {
    var mapEl = document.getElementById('estanciaMap');
    if (!mapEl) return;

    var dados = (window.turismoEstancia && window.turismoEstancia.mapa) || { categorias: [], pontos: [] };
    var categoriaConfig = {};
    dados.categorias.forEach(function (c) {
      categoriaConfig[c.key] = { label: c.label, color: c.color, icon: c.icon };
    });
    var allPois = dados.pontos || [];

    if (!allPois.length) return;

    window.estanciaMarkers = {};
    allPois.forEach(function (p) { window.estanciaMarkers[p.title] = p; });

    // Render Markers
    allPois.forEach(function (poi) {
      var el = document.createElement('div');
      var cls = 'custom-map-marker ' + poi.category;
      if (poi.poi) cls += ' poi';
      el.className = cls;
      el.setAttribute('data-id', poi.id);
      el.setAttribute('data-title', poi.title);
      el.setAttribute('data-category', poi.category);
      el.style.left = poi.left + '%';
      el.style.top = poi.top + '%';
      el.style.animationDelay = poi.delay + 's';
      el.innerHTML = '<div class="custom-map-marker-inner">' + poi.content + '</div><div class="custom-map-marker-tooltip">' + esc(poi.title) + '</div>';
      mapEl.appendChild(el);

      el.addEventListener('click', function () {
        var p = window.estanciaMarkers[this.getAttribute('data-title')];
        if (p) {
          trackAnalytics('mapa-poi', p.id, p.title);
          showPoiInfo(p);
        }
      });
      el.setAttribute('tabindex', '0');
      el.setAttribute('role', 'button');
      el.addEventListener('keydown', function (e) {
        if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); this.click(); }
      });
    });

    // Render Legend
    var legendEl = document.getElementById('mapLegend');
    if (legendEl) {
      legendEl.innerHTML = '';
      dados.categorias.forEach(function (c) {
        var count = allPois.filter(function (p) { return p.category === c.key; }).length;
        if (count === 0) return;
        var item = document.createElement('div');
        item.className = 'map-legend-item';
        item.innerHTML = '<span class="map-legend-dot" style="background:' + c.color + ';"></span><span>' + c.label + '</span>';
        legendEl.appendChild(item);
      });
    }

    // Render Filter Buttons
    var filterBar = document.getElementById('mapFilterBar');
    if (filterBar) {
      filterBar.innerHTML = '';
      var allBtn = document.createElement('button');
      allBtn.className = 'map-filter-btn active';
      allBtn.setAttribute('data-filter', 'all');
      allBtn.innerHTML = 'Todas <span class="map-filter-count">' + allPois.length + '</span>';
      filterBar.appendChild(allBtn);

      dados.categorias.forEach(function (c) {
        var count = allPois.filter(function (p) { return p.category === c.key; }).length;
        if (count === 0) return;
        var btn = document.createElement('button');
        btn.className = 'map-filter-btn';
        btn.setAttribute('data-filter', c.key);
        btn.innerHTML = '<span class="filter-dot" style="background:' + c.color + ';"></span> ' + c.label + ' <span class="map-filter-count">' + count + '</span>';
        filterBar.appendChild(btn);
      });

      filterBar.querySelectorAll('.map-filter-btn').forEach(function (btn) {
        btn.addEventListener('click', function () {
          var filter = this.getAttribute('data-filter');
          filterBar.querySelectorAll('.map-filter-btn').forEach(function (b) { b.classList.remove('active'); });
          this.classList.add('active');
          mapEl.querySelectorAll('.custom-map-marker').forEach(function (m) {
            var cat = m.getAttribute('data-category');
            m.style.display = (filter === 'all' || cat === filter) ? '' : 'none';
          });
        });
      });
    }

    // flyToWonder
    window.flyToWonder = function (title) {
      var poi = window.estanciaMarkers[title];
      if (!poi) return;
      var safeTitle = title.replace(/'/g, "\\'");
      var markerEl = mapEl.querySelector('.custom-map-marker[data-title="' + safeTitle + '"]');
      if (!markerEl) return;
      if (markerEl.style.display === 'none') {
        mapEl.querySelectorAll('.custom-map-marker').forEach(function (m) { m.style.display = ''; });
        var allBtns = document.querySelectorAll('.map-filter-btn');
        allBtns.forEach(function (b) { b.classList.toggle('active', b.getAttribute('data-filter') === 'all'); });
      }
      markerEl.classList.add('highlight');
      setTimeout(function () { markerEl.classList.remove('highlight'); }, 3600);
      mapEl.scrollIntoView({ behavior: 'smooth', block: 'center' });
      setTimeout(function () { showPoiInfo(poi); }, 500);
    };

    function showPoiInfo(poi) {
      var cfg = categoriaConfig[poi.category] || {};
      modalImg.src = poi.img || '';
      modalImg.alt = poi.title;
      modalImg.classList.toggle('hidden', !poi.img);
      modalIcon.setAttribute('data-lucide', poi.icon || 'map-pin');
      modalCategory.textContent = cfg.label || poi.category;
      modalTitle.textContent = poi.title;
      modalDesc.textContent = poi.desc || '';
      modalDetail.textContent = poi.detail || '';
      modalTag.textContent = poi.tag || '';

      if (poi.address) { modalAddressText.textContent = poi.address; modalAddress.classList.add('show'); }
      else { modalAddress.classList.remove('show'); }
      if (poi.directions) { modalDirectionsText.textContent = poi.directions; modalDirections.classList.add('show'); }
      else { modalDirections.classList.remove('show'); }

      setIconWrap(poi.category);
      carregarAvaliacoes(poi.id);

      var oldBtn = document.getElementById('mapViewBtn');
      if (oldBtn) oldBtn.remove();
      if (typeof lucide !== 'undefined') lucide.createIcons();
      modal.classList.add('active');
      document.body.style.overflow = 'hidden';
    }
  }

  initEstanciaMap();

  // ===== Cinematic Scroll Effects =====
  const revealObserver = new IntersectionObserver(function (entries) {
    entries.forEach(function (entry) {
      if (entry.isIntersecting) {
        entry.target.classList.add('is-visible');
        revealObserver.unobserve(entry.target);
      }
    });
  }, { threshold: 0.15, rootMargin: '0px 0px -40px 0px' });

  document.querySelectorAll('.reveal, .reveal-scale, .reveal-left, .reveal-right, .glow-border')
    .forEach(function (el) { revealObserver.observe(el); });

  // 3D tilt on scroll
  const sections3d = document.querySelectorAll('.section-3d');
  let tiltEnabled = false;
  let ticking3d = false;

  function init3DTilt() {
    window.removeEventListener('scroll', onScroll3d);
    tiltEnabled = window.innerWidth >= 1024 && sections3d.length > 0;
    if (tiltEnabled) window.addEventListener('scroll', onScroll3d, { passive: true });
    else sections3d.forEach(function (s) { s.style.transform = ''; });
  }
  function onScroll3d() {
    if (!ticking3d) { requestAnimationFrame(function () { ticking3d = false; }); ticking3d = true; }
  }
  setTimeout(init3DTilt, 600);
  let resizeTimer3d;
  window.addEventListener('resize', function () {
    clearTimeout(resizeTimer3d);
    resizeTimer3d = setTimeout(init3DTilt, 300);
  });

  // Smooth anchor scroll
  const NAVBAR_OFFSET = 80;
  document.querySelectorAll('a[href^="#"]').forEach(function (anchor) {
    anchor.addEventListener('click', function (e) {
      const href = this.getAttribute('href');
      if (href === '#' || href.length < 2) return;
      e.preventDefault();
      const target = document.querySelector(href);
      if (target) {
        const targetPosition = target.getBoundingClientRect().top + window.scrollY - NAVBAR_OFFSET;
        window.scrollTo({ top: targetPosition, behavior: 'smooth' });
      }
    });
  });

  // ===== Hero Particles =====
  function generateHeroParticles() {
    const container = document.getElementById('heroParticles');
    if (!container) return;
    const particleCount = window.innerWidth < 768 ? 15 : 35;
    const types = ['ember', 'sparkle', 'star', 'glow'];
    const emberColors = ['#F97E31', '#FCBB0F', '#ED2027', '#E9568A', '#0095F6'];
    const sparkleColors = ['#FCBB0F', '#F97E31', '#FFFFFF', '#FCD766'];
    const starColors = ['#FFFFFF', '#FFFAF0', '#F0F8FF'];
    const glowColors = ['#F97E31', '#ED2027', '#E9568A'];
    container.innerHTML = '';
    for (let i = 0; i < particleCount; i++) {
      const el = document.createElement('div');
      const type = types[Math.floor(Math.random() * types.length)];
      el.className = 'particle particle--' + type;
      el.style.left = (Math.random() * 100) + '%';
      el.style.top = (10 + Math.random() * 80) + '%';
      let size;
      switch (type) {
        case 'ember': size = 3 + Math.random() * 5; break;
        case 'sparkle': size = 3 + Math.random() * 4; break;
        case 'star': size = 2 + Math.random() * 3; break;
        default: size = 5 + Math.random() * 6;
      }
      el.style.width = size + 'px';
      el.style.height = size + 'px';
      let color;
      switch (type) {
        case 'ember': color = emberColors[Math.floor(Math.random() * emberColors.length)]; break;
        case 'sparkle': color = sparkleColors[Math.floor(Math.random() * sparkleColors.length)]; break;
        case 'star': color = starColors[Math.floor(Math.random() * starColors.length)]; break;
        default: color = glowColors[Math.floor(Math.random() * glowColors.length)];
      }
      el.style.color = color;
      el.style.background = color;
      const duration = 8 + Math.random() * 17;
      const delay = Math.random() * -20;
      el.style.animationDuration = duration + 's';
      el.style.animationDelay = delay + 's';
      container.appendChild(el);
    }
  }
  if (document.readyState === 'complete') generateHeroParticles();
  else window.addEventListener('load', generateHeroParticles);

  // ===== Active Nav Link on Scroll =====
  const navbarLinkEls = document.querySelectorAll('.navbar-link');
  const trackedSections = document.querySelectorAll('[data-section]');

  function updateActiveNavLink() {
    let activeSection = null;
    let maxRatio = 0;
    const vh = window.innerHeight;
    trackedSections.forEach(function (sec) {
      const rect = sec.getBoundingClientRect();
      const visibleTop = Math.max(0, rect.top);
      const visibleBottom = Math.min(vh, rect.bottom);
      const visiblePixels = Math.max(0, visibleBottom - visibleTop);
      const ratio = visiblePixels / (rect.height || 1);
      if (ratio > maxRatio) { maxRatio = ratio; activeSection = sec.dataset.section; }
    });
    if (!activeSection) {
      let minDist = Infinity;
      const vhCenter = vh / 2;
      trackedSections.forEach(function (sec) {
        const rect = sec.getBoundingClientRect();
        const dist = Math.abs((rect.top + rect.height / 2) - vhCenter);
        if (dist < minDist) { minDist = dist; activeSection = sec.dataset.section; }
      });
    }
    navbarLinkEls.forEach(function (link) {
      link.classList.toggle('active', link.dataset.section === activeSection);
    });
  }
  updateActiveNavLink();
  let tickingActive = false;
  window.addEventListener('scroll', function () {
    if (!tickingActive) {
      requestAnimationFrame(function () { updateActiveNavLink(); tickingActive = false; });
      tickingActive = true;
    }
  }, { passive: true });
  window.addEventListener('resize', updateActiveNavLink);

  // ===== Fade-in Reveal for Lazy Images =====
  document.querySelectorAll('img[loading="lazy"]').forEach(function (img) {
    if (img.complete && img.naturalWidth > 0) img.classList.add('loaded');
    else img.addEventListener('load', function () { img.classList.add('loaded'); });
  });

  // ===== Image Fallback Handler =====
  function createFallbackSVG(altText) {
    const safeText = (altText || 'Imagem').replace(/[&<>"']/g, '');
    const svg = '<svg xmlns="http://www.w3.org/2000/svg" width="100%" height="100%" viewBox="0 0 400 300">' +
      '<defs><linearGradient id="fbg" x1="0%" y1="0%" x2="100%" y2="100%">' +
      '<stop offset="0%" stop-color="#0a1628"/><stop offset="100%" stop-color="#1a2a3a"/></linearGradient>' +
      '<radialGradient id="fglow1" cx="30%" cy="30%" r="50%">' +
      '<stop offset="0%" stop-color="rgba(247,100,0,0.12)"/><stop offset="100%" stop-color="transparent"/></radialGradient>' +
      '<radialGradient id="fglow2" cx="70%" cy="70%" r="50%">' +
      '<stop offset="0%" stop-color="rgba(230,43,52,0.10)"/><stop offset="100%" stop-color="transparent"/></radialGradient></defs>' +
      '<rect width="400" height="300" fill="url(#fbg)"/><rect width="400" height="300" fill="url(#fglow1)"/>' +
      '<rect width="400" height="300" fill="url(#fglow2)"/>' +
      '<circle cx="200" cy="120" r="32" fill="rgba(255,255,255,0.06)"/>' +
      '<path d="M185 120 L195 110 L205 110 L215 120 L215 132 L185 132 Z" fill="none" stroke="rgba(255,255,255,0.20)" stroke-width="2" stroke-linejoin="round"/>' +
      '<circle cx="196" cy="120" r="4" fill="rgba(255,255,255,0.15)"/>' +
      '<circle cx="204" cy="120" r="6" fill="rgba(255,255,255,0.10)"/>' +
      '<text x="200" y="190" text-anchor="middle" fill="rgba(255,255,255,0.30)" font-family="Inter,sans-serif" font-size="14">' + safeText + '</text></svg>';
    return 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(svg);
  }

  document.addEventListener('error', function (e) {
    const img = e.target;
    if (img.tagName !== 'IMG') return;
    if (img.classList.contains('img-fallback-processed')) return;
    if (img.src.startsWith('data:image/svg+xml')) return;
    img.classList.add('img-fallback-processed');
    const altText = img.getAttribute('alt') || '';
    img.src = createFallbackSVG(altText);
    img.classList.add('img-fallback');
    img.style.objectFit = 'cover';
  }, true);

  // ===== Vitrine das 7 Maravilhas (baralho de cartas com prévia) =====
  // Cada maravilha é uma carta no deck de scroll-snap: a central é a exibida,
  // a próxima espreita pela direita e a anterior fica à esquerda. Arraste,
  // setas ou teclado passam as cartas — o scroll define a carta atual.
  function initWondersVitrine() {
    const vitrine = document.getElementById('wondersVitrine');
    if (!vitrine) return;
    const deck = document.getElementById('vitrineDeck');
    const cartas = Array.prototype.slice.call(vitrine.querySelectorAll('.vitrine-carta'));
    const total = cartas.length;
    if (total === 0 || !deck) return;

    let atual = 0;

    // A carta mais próxima do centro do deck é a atual: ela escala para 1,
    // as laterais encolhem/apagam e o contador acompanha.
    let raf = null;
    function atualizarEstados() {
      const centro = deck.scrollLeft + deck.clientWidth / 2;
      let melhor = 0;
      let melhorDist = Infinity;
      cartas.forEach(function (c, i) {
        const cc = c.offsetLeft + c.offsetWidth / 2;
        const dist = Math.abs(cc - centro);
        if (dist < melhorDist) { melhorDist = dist; melhor = i; }
      });
      atual = melhor;
      cartas.forEach(function (c, i) {
        const ativa = i === melhor;
        c.classList.toggle('is-atual', ativa);
        c.classList.toggle('is-lateral', !ativa);
        if (ativa) c.setAttribute('aria-current', 'true');
        else c.removeAttribute('aria-current');
      });
    }

    function noScroll() {
      if (raf) return;
      raf = requestAnimationFrame(function () {
        raf = null;
        atualizarEstados();
      });
    }
    deck.addEventListener('scroll', noScroll, { passive: true });
    if ('onscrollend' in window) deck.addEventListener('scrollend', atualizarEstados);

    function irPara(indice) {
      const alvo = cartas[(indice + total) % total];
      alvo.scrollIntoView({ behavior: 'smooth', inline: 'center', block: 'nearest' });
    }

    // Clicar numa carta lateral (fora de links/botões) leva a carta ao centro:
    // a prévia da próxima avança e a anterior volta.
    cartas.forEach(function (c, i) {
      c.addEventListener('click', function (e) {
        if (i === atual) return;
        const alvo = e.target;
        if (alvo && alvo.closest && alvo.closest('a, button')) return;
        irPara(i);
      });
    });

    const prevBtn = document.getElementById('vitrinePrev');
    const nextBtn = document.getElementById('vitrineNext');
    if (prevBtn) prevBtn.addEventListener('click', function () { irPara(atual - 1); });
    if (nextBtn) nextBtn.addEventListener('click', function () { irPara(atual + 1); });

    document.addEventListener('keydown', function (e) {
      // Só navega quando a vitrine está visível na tela (a home tem duas vitrines).
      if (!vitrine.closest('.section-wonders, .paginas-container')) return;
      const rect = vitrine.getBoundingClientRect();
      const vh = window.innerHeight;
      if (rect.bottom < 0 || rect.top > vh) return;
      // Não navega a vitrine enquanto o usuário digita em campos de formulário.
      const alvo = e.target;
      if (alvo && (alvo.tagName === 'INPUT' || alvo.tagName === 'TEXTAREA' || alvo.tagName === 'SELECT' || alvo.isContentEditable)) return;
      if (e.key === 'ArrowRight') { irPara(atual + 1); e.preventDefault(); }
      if (e.key === 'ArrowLeft') { irPara(atual - 1); e.preventDefault(); }
    });

    // Arraste com o mouse no desktop (no celular o scroll-snap nativo já passa
    // as cartas). Durante o arrasto o snap é desativado (.is-dragging) para o
    // dedo/mouse mover livremente; ao soltar, a carta mais próxima é encaixada.
    // Um clique que não arrastou continua normal; depois de arrastar, o clique
    // é suprimido para não abrir links/cartas.
    let arrastando = false;
    let iniX = 0;
    let iniScroll = 0;
    let moveu = false;
    deck.addEventListener('mousedown', function (e) {
      if (e.button !== 0) return;
      if (e.target.closest && e.target.closest('a, button')) return;
      arrastando = true;
      moveu = false;
      iniX = e.clientX;
      iniScroll = deck.scrollLeft;
      deck.classList.add('is-dragging');
    });
    window.addEventListener('mousemove', function (e) {
      if (!arrastando) return;
      const dx = e.clientX - iniX;
      if (Math.abs(dx) > 4) moveu = true;
      deck.scrollLeft = iniScroll - dx;
    });
    window.addEventListener('mouseup', function () {
      if (!arrastando) return;
      arrastando = false;
      deck.classList.remove('is-dragging');
      // Encaixa na carta mais próxima do centro ao soltar.
      const centro = deck.scrollLeft + deck.clientWidth / 2;
      let melhor = 0;
      let melhorDist = Infinity;
      cartas.forEach(function (c, i) {
        const d = Math.abs((c.offsetLeft + c.offsetWidth / 2) - centro);
        if (d < melhorDist) { melhorDist = d; melhor = i; }
      });
      irPara(melhor);
    });
    deck.addEventListener('click', function (e) {
      if (moveu) {
        e.preventDefault();
        e.stopPropagation();
        moveu = false;
      }
    }, true);

    atualizarEstados();
    // Recalcula quando o layout estabiliza (CSS/imagens carregadas) e na
    // troca de tamanho de tela (offsetLeft depende da largura do deck).
    requestAnimationFrame(atualizarEstados);
    window.addEventListener('load', atualizarEstados);
    window.addEventListener('resize', atualizarEstados);
  }

  initWondersVitrine();

  // ===== Galeria: lightbox =====
  // As fotos da galeria (/galeria) abrem num visualizador fullscreen com
  // navegação por setas/teclado; o grid usa os thumbnails e o lightbox
  // carrega a imagem otimizada (1600px) sob demanda.
  (function initGaleriaLightbox() {
    const lightbox = document.getElementById('galeriaLightbox');
    if (!lightbox) return;

    const img = document.getElementById('galeriaLightboxImg');
    const legenda = document.getElementById('galeriaLightboxLegenda');
    const contador = document.getElementById('galeriaLightboxContador');
    const fechar = document.getElementById('galeriaLightboxFechar');
    const prevBtn = document.getElementById('galeriaLightboxPrev');
    const nextBtn = document.getElementById('galeriaLightboxNext');
    const curtirBtn = document.getElementById('galeriaLightboxCurtir');
    const curtidasEl = document.getElementById('galeriaLightboxCurtidas');
    const visualizacoesEl = document.getElementById('galeriaLightboxVisualizacoes');
    const itens = Array.prototype.slice.call(document.querySelectorAll('.galeria-item'));
    if (!itens.length) return;
    let atual = -1;

    // Ids curtidos nesta sessão (memória local — só para o estado visual do
    // botão; o dedup real é server-side por cookie de sessão).
    let curtidos = new Set();
    try {
      const salvo = JSON.parse(sessionStorage.getItem('galeriaCurtidas') || '[]');
      curtidos = new Set(salvo);
    } catch (e) { /* sessionStorage indisponível */ }

    function salvarCurtidos() {
      try { sessionStorage.setItem('galeriaCurtidas', JSON.stringify(Array.from(curtidos))); } catch (e) { /* noop */ }
    }

    function tokenAntiForgery() {
      const el = lightbox.querySelector('input[name="__RequestVerificationToken"]');
      return el ? el.value : '';
    }

    function atualizarCurtir(item) {
      if (!curtirBtn) return;
      const curtidas = parseInt(item.getAttribute('data-like'), 10) || 0;
      const jaCurtiu = curtidos.has(item.getAttribute('data-id'));
      curtidasEl.textContent = String(curtidas);
      curtirBtn.classList.toggle('is-curtido', jaCurtiu);
      curtirBtn.setAttribute('aria-pressed', jaCurtiu ? 'true' : 'false');
      curtirBtn.disabled = jaCurtiu;
    }

    function abrir(indice) {
      if (indice < 0 || indice >= itens.length) return;
      atual = indice;
      const item = itens[atual];
      // Fade-in da foto no lightbox: esconde até o load disparar.
      img.classList.remove('is-loaded');
      img.onload = function () { img.classList.add('is-loaded'); };
      img.src = item.getAttribute('data-full') || '';
      img.alt = item.getAttribute('data-titulo') || '';
      legenda.textContent = item.getAttribute('data-titulo') || '';
      contador.textContent = (atual + 1) + ' / ' + itens.length;
      visualizacoesEl.textContent = item.getAttribute('data-views') || '0';
      atualizarCurtir(item);
      lightbox.classList.add('active');
      lightbox.setAttribute('aria-hidden', 'false');
      document.body.style.overflow = 'hidden';
      registrarVisualizacao(item);
    }

    // Visualização (lightbox aberto): conta no servidor + evento de analytics.
    function registrarVisualizacao(item) {
      const id = item.getAttribute('data-id');
      trackAnalytics('visualizacao-foto', id, item.getAttribute('data-titulo'));
      if (!navigator.sendBeacon && !window.fetch) return;
      // App pode rodar sob sub-pasta (ex.: /turismo) — usa o base do PathBase.
      const base = lightbox.getAttribute('data-app-base') || '';
      fetch(base + 'galeria/visualizar/' + id, {
        method: 'POST',
        headers: {
          'RequestVerificationToken': tokenAntiForgery(),
          'X-Requested-With': 'XMLHttpRequest'
        }
      }).then(function (r) { return r.ok ? r.json() : null; })
        .then(function (d) {
          if (d && d.visualizacoes) {
            item.setAttribute('data-views', String(d.visualizacoes));
            visualizacoesEl.textContent = String(d.visualizacoes);
          }
        })
        .catch(function () { /* contagem é não-crítica */ });
    }

    // Curtida "Amei": dedup por sessão no servidor.
    if (curtirBtn) {
      curtirBtn.addEventListener('click', function () {
        if (curtirBtn.disabled) return;
        const item = itens[atual];
        const id = item.getAttribute('data-id');
        const base = lightbox.getAttribute('data-app-base') || '';
        fetch(base + 'galeria/curtir/' + id, {
          method: 'POST',
          headers: {
            'RequestVerificationToken': tokenAntiForgery(),
            'X-Requested-With': 'XMLHttpRequest'
          }
        }).then(function (r) { return r.ok ? r.json() : null; })
          .then(function (d) {
            if (!d) return;
            item.setAttribute('data-like', String(d.curtidas));
            curtidasEl.textContent = String(d.curtidas);
            if (d.jaCurtiu) {
              curtidos.add(id);
              salvarCurtidos();
              atualizarCurtir(item);
            }
            if (typeof lucide !== 'undefined') lucide.createIcons();
          })
          .catch(function () { /* noop */ });
      });
    }

    function fecharLightbox() {
      lightbox.classList.remove('active');
      lightbox.setAttribute('aria-hidden', 'true');
      document.body.style.overflow = '';
      img.src = '';
    }

    function navegar(delta) {
      abrir((atual + delta + itens.length) % itens.length);
    }

    itens.forEach(function (item, i) {
      item.addEventListener('click', function (e) {
        e.preventDefault();
        trackAnalytics('galeria-foto', item.getAttribute('data-id'), item.getAttribute('data-titulo'));
        abrir(i);
      });
    });

    if (fechar) fechar.addEventListener('click', fecharLightbox);
    if (prevBtn) prevBtn.addEventListener('click', function () { navegar(-1); });
    if (nextBtn) nextBtn.addEventListener('click', function () { navegar(1); });
    lightbox.addEventListener('click', function (e) {
      if (e.target === lightbox) fecharLightbox();
    });
    document.addEventListener('keydown', function (e) {
      if (!lightbox.classList.contains('active')) return;
      if (e.key === 'Escape') fecharLightbox();
      if (e.key === 'ArrowLeft') navegar(-1);
      if (e.key === 'ArrowRight') navegar(1);
    });
  })();

  // ===== Galeria: placeholder + fade-in das imagens =====
  // As fotos do grid e as capas dos cards começam transparentes sobre um
  // shimmer (CSS) e ganham .is-carregada quando o load dispara — sem flash
  // branco e sem layout shift. Cobre também o caso de imagem já em cache
  // (complete + naturalWidth) e o de imagem indisponível (.is-erro).
  (function initGaleriaPlaceholder() {
    const imgs = document.querySelectorAll('.paginas-galeria-grid--fotos .galeria-item img, .paginas-galeria-cards .paginas-galeria-card img');
    if (!imgs.length) return;

    function marcarCarregada(img) {
      img.classList.add('is-loaded');
      const item = img.closest('.galeria-item, .paginas-galeria-card');
      if (item) item.classList.add('is-carregada');
    }

    imgs.forEach(function (img) {
      if (img.complete && img.naturalWidth > 0) {
        marcarCarregada(img);
      } else {
        img.addEventListener('load', function () { marcarCarregada(img); });
        img.addEventListener('error', function () {
          const item = img.closest('.galeria-item, .paginas-galeria-card');
          if (item) item.classList.add('is-erro');
        });
      }
    });
  })();

  // ===== Analytics: beacon de cliques (anônimo, LGPD-safe) =====
  // Envia o clique via sendBeacon — nunca bloqueia a navegação. A sessão
  // anônima vem do cookie te_sessao; nada pessoal trafega no payload.
  // O app pode rodar sob sub-pasta (ex.: /turismo) — a base vem do <body>.
  function appBase() {
    var b = document.body && document.body.getAttribute('data-app-base');
    if (b) return b;
    var lb = document.getElementById('galeriaLightbox');
    return (lb && lb.getAttribute('data-app-base')) || '';
  }

  function trackAnalytics(evento, entidadeId, entidadeNome) {
    try {
      if (!navigator.sendBeacon) return;
      var payload = JSON.stringify({
        evento: evento || null,
        entidadeId: entidadeId || null,
        entidadeNome: entidadeNome || null,
        rota: window.location.pathname
      });
      navigator.sendBeacon(appBase() + 'api/analytics/event', new Blob([payload], { type: 'application/json' }));
    } catch (e) { /* nunca bloqueia o clique */ }
  }

  // Qualquer elemento com data-track é rastreado (Ver detalhes, cards...).
  document.addEventListener('click', function (e) {
    var alvo = e.target && e.target.closest ? e.target.closest('[data-track]') : null;
    if (!alvo) return;
    trackAnalytics(
      alvo.getAttribute('data-track'),
      alvo.getAttribute('data-track-id'),
      alvo.getAttribute('data-track-nome')
    );
  });

  // ===== Helper =====
  function esc(texto) {
    return String(texto == null ? '' : texto)
      .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;').replace(/'/g, '&#39;');
  }

  console.log('🎬 Descubra Estância — portal dinâmico carregado');
});
