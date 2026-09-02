/**
 * ABEC shell layout (Admin + Student):
 * - On every module page open, the sidebar auto-hides for a focused full-width layout
 * - Reveal via left-edge hover / "Menu" hint; hide again when the pointer enters main content
 * - Secondary screens (User Settings, Course Detail, Schedule, etc.) stay full-screen with a Back button
 *
 * API:
 *   AbecShell.enterSecondary({ backLabel, onBack, skipInjectedBack })
 *   AbecShell.exitSecondary()
 *   AbecShell.hideSidebar()
 *   AbecShell.showSidebar()
 *   AbecShell.isSecondary()
 *
 * Declarative secondary page:
 *   <body data-abec-shell="secondary" data-abec-back="..." data-abec-back-label="...">
 */
(function () {
  'use strict';

  var AUTO_CLASS = 'sidebar-auto-hidden';
  var FOCUS_CLASS = 'sidebar-focus-mode';
  var HIDE_DELAY_MS = 140;
  var EDGE_PX = 18;

  var state = {
    secondary: false,
    onBack: null,
    backLabel: 'Back',
    injectedBack: null,
    hideTimer: null,
    hoveringSidebar: false
  };

  function injectStyles() {
    if (document.getElementById('abec-shell-styles')) return;
    var style = document.createElement('style');
    style.id = 'abec-shell-styles';
    style.textContent = [
      '.app-sidebar {',
      '  transition: transform 0.35s cubic-bezier(0.22, 1, 0.36, 1), opacity 0.28s ease;',
      '  will-change: transform;',
      '}',
      'body.' + AUTO_CLASS + ' .app-sidebar,',
      'body.' + FOCUS_CLASS + ' .app-sidebar {',
      '  transform: translateX(-100%);',
      '  opacity: 0;',
      '  pointer-events: none;',
      '}',
      '.app-main, .abec-main {',
      '  transition: margin-left 0.35s cubic-bezier(0.22, 1, 0.36, 1);',
      '}',
      'body.' + AUTO_CLASS + ' .app-main,',
      'body.' + AUTO_CLASS + ' .abec-main,',
      'body.' + AUTO_CLASS + ' main,',
      'body.' + FOCUS_CLASS + ' .app-main,',
      'body.' + FOCUS_CLASS + ' .abec-main,',
      'body.' + FOCUS_CLASS + ' main {',
      '  margin-left: 0 !important;',
      '}',
      '.sidebar-hotzone {',
      '  position: fixed; left: 0; top: 0; width: ' + EDGE_PX + 'px; height: 100%; z-index: 45;',
      '}',
      'body:not(.' + AUTO_CLASS + '):not(.' + FOCUS_CLASS + ') .sidebar-hotzone {',
      '  pointer-events: none;',
      '}',
      '.sidebar-hint {',
      '  position: fixed; top: 50%; left: 0; z-index: 50;',
      '  display: inline-flex; align-items: center; justify-content: center;',
      '  margin: 0; padding: 0.7rem 0.35rem;',
      '  border: 1px solid #E8EEF5; border-left: none;',
      '  border-radius: 0 0.75rem 0.75rem 0;',
      '  background: #fff; color: #64748B;',
      '  font-size: 0.625rem; font-weight: 650; letter-spacing: 0.06em;',
      '  text-transform: uppercase; writing-mode: vertical-rl; text-orientation: mixed;',
      '  transform: translate(-0.2rem, -50%);',
      '  box-shadow: 0 1px 2px rgba(15,23,42,0.04), 0 8px 20px rgba(15,23,42,0.05);',
      '  opacity: 0; pointer-events: none;',
      '  transition: opacity 0.28s ease, transform 0.28s ease, color 0.2s ease, background-color 0.2s ease, border-color 0.2s ease;',
      '  font-family: inherit; line-height: 1; cursor: pointer;',
      '}',
      'body.' + AUTO_CLASS + ' .sidebar-hint,',
      'body.' + FOCUS_CLASS + ' .sidebar-hint {',
      '  opacity: 1; pointer-events: auto; transform: translate(0, -50%);',
      '}',
      /* User Settings / declarative secondary pages: hide Menu hint */
      'body[data-abec-shell="secondary"] .sidebar-hint {',
      '  display: none !important;',
      '  opacity: 0 !important;',
      '  pointer-events: none !important;',
      '}',
      '.sidebar-hint:hover {',
      '  background: #F7F9FC; border-color: #D7E3F0; color: #0A2540;',
      '}',
      '.abec-back-btn {',
      '  display: inline-flex; align-items: center; gap: 0.45rem;',
      '  margin-bottom: 1.1rem; padding: 0.5rem 0.85rem;',
      '  border-radius: 0.75rem; border: 1px solid #E8EEF5; background: #fff; color: #0A2540;',
      '  font-size: 0.8125rem; font-weight: 600; letter-spacing: -0.01em; text-decoration: none;',
      '  box-shadow: 0 1px 2px rgba(15,23,42,0.03), 0 8px 20px rgba(15,23,42,0.04);',
      '  transition: background 0.22s ease, border-color 0.22s ease, box-shadow 0.22s ease, transform 0.22s ease;',
      '  cursor: pointer; font-family: inherit;',
      '}',
      '.abec-back-btn:hover {',
      '  background: #F7F9FC; border-color: #D7E3F0;',
      '  box-shadow: 0 4px 14px rgba(15,23,42,0.06); transform: translateY(-1px);',
      '}',
      '.abec-back-btn svg { width: 1rem; height: 1rem; flex-shrink: 0; }',
      '.abec-back-bar { display: flex; align-items: center; justify-content: flex-start; }',
      'body:not(.' + FOCUS_CLASS + ') .abec-back-btn[data-abec-injected="true"] { display: none !important; }'
    ].join('\n');
    document.head.appendChild(style);
  }

  function findSidebar() {
    return (
      document.querySelector('aside[data-purpose="main-sidebar"]') ||
      document.querySelector('aside.abec-sidebar') ||
      document.querySelector('aside.w-64.fixed') ||
      document.querySelector('aside.fixed.h-full.z-20') ||
      document.querySelector('aside.w-64.bg-abec-navy')
    );
  }

  function findMain() {
    return document.getElementById('main-shell') || document.querySelector('main');
  }

  function ensureShellClasses() {
    var sidebar = findSidebar();
    var main = findMain();
    if (sidebar) sidebar.classList.add('app-sidebar');
    if (main) main.classList.add('app-main');
    return { sidebar: sidebar, main: main };
  }

  function isHidden() {
    return document.body.classList.contains(AUTO_CLASS) || document.body.classList.contains(FOCUS_CLASS);
  }

  function showSidebar() {
    clearTimeout(state.hideTimer);
    state.hideTimer = null;
    document.body.classList.remove(AUTO_CLASS);
    // Keep FOCUS_CLASS if in secondary — revealing nav temporarily while secondary
    // still allowed; removing only auto class lets sidebar slide in over focus layout.
  }

  function hideSidebar() {
    if (state.hoveringSidebar) return;
    clearTimeout(state.hideTimer);
    state.hideTimer = setTimeout(function () {
      if (state.hoveringSidebar) return;
      var openModal = document.querySelector('[role="dialog"]:not(.hidden), [aria-modal="true"]:not(.hidden)');
      if (openModal) return;
      document.body.classList.add(AUTO_CLASS);
    }, HIDE_DELAY_MS);
  }

  function forceHideSidebar() {
    clearTimeout(state.hideTimer);
    state.hideTimer = null;
    document.body.classList.add(AUTO_CLASS);
  }

  function createBackButton(label) {
    var btn = document.createElement('button');
    btn.type = 'button';
    btn.className = 'abec-back-btn';
    btn.setAttribute('data-abec-injected', 'true');
    btn.innerHTML =
      '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">' +
        '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75" d="M15 19l-7-7 7-7"></path>' +
      '</svg><span></span>';
    btn.querySelector('span').textContent = label || 'Back';
    btn.addEventListener('click', function () {
      if (typeof state.onBack === 'function') {
        state.onBack();
        return;
      }
      var href = document.body.getAttribute('data-abec-back');
      if (href) {
        window.location.href = href;
        return;
      }
      if (window.history.length > 1) window.history.back();
    });
    return btn;
  }

  function ensureInjectedBack(label) {
    var main = findMain();
    if (!main) return null;
    if (main.querySelector('[data-abec-back-btn], .abec-back-btn:not([data-abec-injected])')) {
      return null;
    }
    if (state.injectedBack && document.body.contains(state.injectedBack)) {
      var span = state.injectedBack.querySelector('span');
      if (span) span.textContent = label || 'Back';
      return state.injectedBack;
    }
    var btn = createBackButton(label);
    var bar = document.createElement('div');
    bar.className = 'abec-back-bar';
    bar.setAttribute('data-abec-injected', 'true');
    bar.appendChild(btn);
    main.insertBefore(bar, main.firstChild);
    state.injectedBack = btn;
    return btn;
  }

  function removeInjectedBack() {
    if (!state.injectedBack) return;
    var bar = state.injectedBack.closest('.abec-back-bar');
    if (bar && bar.getAttribute('data-abec-injected') === 'true') bar.remove();
    else if (state.injectedBack.parentNode) state.injectedBack.remove();
    state.injectedBack = null;
  }

  function enterSecondary(options) {
    options = options || {};
    injectStyles();
    ensureShellClasses();

    state.secondary = true;
    state.onBack = typeof options.onBack === 'function' ? options.onBack : null;
    state.backLabel = options.backLabel || document.body.getAttribute('data-abec-back-label') || 'Back';

    document.body.classList.add(FOCUS_CLASS);
    document.body.classList.add(AUTO_CLASS);
    document.body.setAttribute('data-abec-shell-active', 'secondary');

    if (options.injectBack !== false && !options.skipInjectedBack) {
      var main = findMain();
      var hasPageBack = main && main.querySelector('[data-abec-back-btn]');
      if (!hasPageBack) ensureInjectedBack(state.backLabel);
    }

    window.scrollTo(0, 0);
  }

  function exitSecondary() {
    state.secondary = false;
    state.onBack = null;
    document.body.classList.remove(FOCUS_CLASS);
    document.body.removeAttribute('data-abec-shell-active');
    removeInjectedBack();
    // Return to default module behavior: sidebar stays auto-hidden
    forceHideSidebar();
  }

  function styleExistingBackButtons() {
    var main = findMain();
    if (!main) return;
    main.querySelectorAll('a[onclick*="showView(\'overview\'"], button[onclick*="showView(\'overview\'"]').forEach(function (el) {
      el.classList.add('abec-back-btn');
      el.setAttribute('data-abec-back-btn', 'true');
    });
  }

  function setupAutoHide(sidebar, main) {
    if (!sidebar || !main) return;

    var hotzone = document.createElement('div');
    hotzone.className = 'sidebar-hotzone';
    hotzone.setAttribute('aria-hidden', 'true');
    document.body.appendChild(hotzone);

    var hint = document.createElement('button');
    hint.type = 'button';
    hint.className = 'sidebar-hint';
    hint.title = 'Show navigation menu';
    hint.setAttribute('aria-label', 'Show navigation menu');
    hint.textContent = '‹ Menu';
    document.body.appendChild(hint);

    sidebar.addEventListener('mouseenter', function () {
      state.hoveringSidebar = true;
      showSidebar();
    });
    sidebar.addEventListener('mouseleave', function () {
      state.hoveringSidebar = false;
    });

    hotzone.addEventListener('mouseenter', showSidebar);
    hint.addEventListener('mouseenter', showSidebar);
    hint.addEventListener('click', showSidebar);
    main.addEventListener('mouseenter', hideSidebar);

    document.addEventListener('mousemove', function (e) {
      if (e.clientX <= EDGE_PX) showSidebar();
    });

    // Touch / no-hover devices: keep a way to toggle via hint click only;
    // still start hidden for a focused layout.
    if (window.matchMedia && window.matchMedia('(hover: none)').matches) {
      hint.addEventListener('click', function () {
        if (isHidden()) showSidebar();
        else forceHideSidebar();
      });
    }
  }

  function initDeclarativeSecondaryPage() {
    var mode = (document.body.getAttribute('data-abec-shell') || '').toLowerCase();
    if (mode === 'secondary' || mode === 'focus') {
      enterSecondary({
        backLabel: document.body.getAttribute('data-abec-back-label') || 'Back',
        onBack: function () {
          var href = document.body.getAttribute('data-abec-back');
          if (href) window.location.href = href;
          else if (window.history.length > 1) window.history.back();
        }
      });
      return true;
    }
    return false;
  }

  function init() {
    injectStyles();
    var shell = ensureShellClasses();
    if (!shell.sidebar || !shell.main) return;

    styleExistingBackButtons();
    setupAutoHide(shell.sidebar, shell.main);

    var isSecondaryPage = initDeclarativeSecondaryPage();
    if (!isSecondaryPage) {
      // All module pages: hide sidebar as soon as the screen opens
      forceHideSidebar();
    }
  }

  window.AbecShell = {
    enterSecondary: enterSecondary,
    exitSecondary: exitSecondary,
    hideSidebar: forceHideSidebar,
    showSidebar: showSidebar,
    isSecondary: function () { return state.secondary; },
    refresh: function () {
      ensureShellClasses();
      styleExistingBackButtons();
    }
  };

  window.showAdminSidebar = showSidebar;
  window.hideAdminSidebar = forceHideSidebar;

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();
