window.nerdShared = window.nerdShared || {
  copyText: async (text) => {
    const fallbackCopy = () => {
      const textarea = document.createElement('textarea');
      textarea.value = text;
      textarea.setAttribute('readonly', '');
      textarea.style.position = 'fixed';
      textarea.style.left = '-9999px';
      document.body.appendChild(textarea);
      textarea.select();
      document.execCommand('copy');
      document.body.removeChild(textarea);
    };

    try {
      if (navigator.clipboard?.writeText) {
        await Promise.race([
          navigator.clipboard.writeText(text),
          new Promise((_, reject) => setTimeout(() => reject(new Error('clipboard-timeout')), 750)),
        ]);
        return;
      }
    } catch {
      // Fall back when permissions are missing or clipboard hangs (common in automation).
    }

    fallbackCopy();
  },
  scrollToElement: (elementId) => {
    const element = document.getElementById(elementId);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  },
  downloadText: (fileName, text) => {
    const blob = new Blob([text], { type: 'text/plain;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    anchor.click();
    URL.revokeObjectURL(url);
  },
  downloadBytes: (fileName, base64, contentType) => {
    const binary = atob(base64);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
      bytes[i] = binary.charCodeAt(i);
    }
    const blob = new Blob([bytes], { type: contentType || 'application/octet-stream' });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    anchor.click();
    URL.revokeObjectURL(url);
  }
};

(() => {
  if (window.nerdShared.portalTokenObserver) {
    return;
  }

  let activeToken;
  let applyingPortalToken = false;

  const addTokenToPopover = (popover) => {
    if (!activeToken || !popover?.classList || popover.classList.contains(activeToken)) {
      return;
    }

    popover.classList.add(activeToken);
  };

  const applyTokenToPopovers = (token, previousToken) => {
    if (!token || applyingPortalToken) {
      return;
    }

    applyingPortalToken = true;
    try {
      document.querySelectorAll('.mud-popover, .mud-picker-popover').forEach((popover) => {
        if (previousToken) {
          popover.classList.remove(previousToken);
        }

        addTokenToPopover(popover);
      });
    } finally {
      applyingPortalToken = false;
    }
  };
  const setActiveToken = (tokenHost) => {
    const token = tokenHost?.dataset?.nerdToken;
    if (!token || token === activeToken) {
      return;
    }

    const previousToken = activeToken;
    activeToken = token;
    applyTokenToPopovers(activeToken, previousToken);
  };
  const copyTokenToPopovers = (root) => {
    if (!activeToken || applyingPortalToken) {
      return;
    }

    const popovers = [];
    if (root.matches?.('.mud-popover, .mud-picker-popover')) {
      popovers.push(root);
    }
    root.querySelectorAll?.('.mud-popover, .mud-picker-popover').forEach((popover) => {
      popovers.push(popover);
    });
    popovers.forEach(addTokenToPopover);
  };

  document.addEventListener('pointerdown', (event) => {
    const tokenHost = event.target.closest?.('[data-nerd-token]');
    setActiveToken(tokenHost);
  }, true);

  document.addEventListener('focusin', (event) => {
    const tokenHost = event.target.closest?.('[data-nerd-token]');
    setActiveToken(tokenHost);
  }, true);

  const observer = new MutationObserver((mutations) => {
    for (const mutation of mutations) {
      if (mutation.type === 'attributes' && mutation.attributeName === 'class') {
        const target = mutation.target;
        if (target.nodeType === Node.ELEMENT_NODE && target.matches?.('.mud-popover, .mud-picker-popover')) {
          copyTokenToPopovers(target);
        }
      }

      mutation.addedNodes.forEach((node) => {
        if (node.nodeType === Node.ELEMENT_NODE) {
          copyTokenToPopovers(node);
        }
      });
    }
  });

  observer.observe(document.body, {
    childList: true,
    subtree: true,
    attributes: true,
    attributeFilter: ['class']
  });
  window.nerdShared.portalTokenObserver = observer;
  window.nerdShared.setPortalToken = (token) => {
    if (!token || token === activeToken) {
      return;
    }

    const previousToken = activeToken;
    activeToken = token;
    applyTokenToPopovers(activeToken, previousToken);
  };

  const parseRgb = (color) => {
    const value = `${color}`.trim();
    const rgbaMatch = value.match(/rgba?\((\d+),\s*(\d+),\s*(\d+)/i);
    if (rgbaMatch) {
      return { r: Number(rgbaMatch[1]), g: Number(rgbaMatch[2]), b: Number(rgbaMatch[3]) };
    }

    const srgbMatch = value.match(/color\(\s*srgb\s+([\d.]+)\s+([\d.]+)\s+([\d.]+)/i);
    if (srgbMatch) {
      return {
        r: Math.round(Number(srgbMatch[1]) * 255),
        g: Math.round(Number(srgbMatch[2]) * 255),
        b: Math.round(Number(srgbMatch[3]) * 255),
      };
    }

    return null;
  };

  const relativeLuminance = ({ r, g, b }) => {
    const channel = (value) => {
      const normalized = value / 255;
      return normalized <= 0.03928
        ? normalized / 12.92
        : ((normalized + 0.055) / 1.055) ** 2.4;
    };

    return 0.2126 * channel(r) + 0.7152 * channel(g) + 0.0722 * channel(b);
  };

  const contrastRatio = (foreground, background) => {
    const fg = parseRgb(foreground);
    const bg = parseRgb(background);
    if (!fg || !bg) {
      return 0;
    }

    const lighter = Math.max(relativeLuminance(fg), relativeLuminance(bg));
    const darker = Math.min(relativeLuminance(fg), relativeLuminance(bg));
    return (lighter + 0.05) / (darker + 0.05);
  };

  const resolveBackgroundColor = (element) => {
    let node = element;
    while (node) {
      const background = getComputedStyle(node).backgroundColor;
      if (background && background !== 'rgba(0, 0, 0, 0)' && background !== 'transparent') {
        return background;
      }
      node = node.parentElement;
    }

    return getComputedStyle(document.body).backgroundColor;
  };

  const styleGuardState = {
    enabled: false,
    errorRatio: 4.5,
    warningRatio: 3,
    scanSelector: '[data-nerd-style-guard]',
    highlighted: [],
    clickBound: false,
    defaultOptions: {
      errorRatio: 4.5,
      warningRatio: 3,
      scanSelector: '[data-nerd-style-guard]',
    },
  };

  const isWhite = (color) => color === 'rgb(255, 255, 255)' || color === 'rgba(255, 255, 255, 1)';

  const isTabElement = (element) =>
    element.matches('.mud-tab, [role="tab"]');

  const classifyContrast = (element, ratio, foreground, background) => {
    if (isWhite(foreground) && isWhite(background)) {
      return 'error';
    }

    if (isTabElement(element)) {
      return ratio + 0.0001 < styleGuardState.warningRatio ? 'warning' : null;
    }

    return ratio + 0.0001 < styleGuardState.errorRatio ? 'error' : null;
  };

  const classifySurfaceContrast = (ratio, foreground, background) => {
    if (isWhite(foreground) && isWhite(background)) {
      return 'error';
    }

    return ratio + 0.0001 < styleGuardState.warningRatio ? 'error' : null;
  };

  const escapeHtml = (value) =>
    `${value}`
      .replaceAll('&', '&amp;')
      .replaceAll('<', '&lt;')
      .replaceAll('>', '&gt;')
      .replaceAll('"', '&quot;');

  const ensureStyleGuardPanel = () => {
    let panel = document.getElementById('nerd-style-guard-panel');
    if (!panel) {
      panel = document.createElement('div');
      panel.id = 'nerd-style-guard-panel';
      panel.setAttribute('role', 'dialog');
      panel.setAttribute('aria-label', 'Style guard results');
      panel.style.cssText =
        'position:fixed;bottom:48px;right:12px;z-index:100001;max-width:380px;max-height:42vh;overflow:auto;padding:10px 12px;border-radius:8px;font:12px/1.45 sans-serif;background:#111827;color:#fff;box-shadow:0 8px 24px rgba(0,0,0,.28);display:none;pointer-events:auto;';
      document.body.appendChild(panel);
    }

    return panel;
  };

  const updateStyleGuardPanel = (summary) => {
    const panel = ensureStyleGuardPanel();
    const badge = document.getElementById('nerd-style-guard-badge');
    const isOpen = panel.dataset.open === 'true';

    if (isOpen) {
      panel.dataset.open = 'false';
      panel.style.display = 'none';
      badge?.removeAttribute('data-guard-panel-open');
      return;
    }

    const lines =
      summary.items.length === 0
        ? ['<p style="margin:0">No contrast issues in catalog chrome or toolbars.</p>']
        : summary.items.map(
            (item) =>
              `<p style="margin:0 0 8px"><strong>${escapeHtml(item.level)}</strong>: ${escapeHtml(item.text)} (${item.ratio.toFixed(2)}:1)</p>`,
          );

    panel.innerHTML = `<div style="font-weight:600;margin-bottom:8px">Style guard results</div>${lines.join('')}`;
    panel.dataset.open = 'true';
    panel.style.display = 'block';
    badge?.setAttribute('data-guard-panel-open', 'true');
    badge?.setAttribute('data-guard-last-scan', String(Date.now()));
  };

  const updateStyleGuardBadge = (summary) => {
    const badge = document.getElementById('nerd-style-guard-badge');
    if (!badge) {
      return;
    }

    if (summary.errors === 0 && summary.warnings === 0) {
      badge.textContent = 'Style guard ✓';
      badge.style.background = '#166534';
      badge.dataset.guardStatus = 'ok';
      return;
    }

    if (summary.errors > 0) {
      const suffix = summary.errors === 1 ? 'error' : 'errors';
      badge.textContent = `Style guard: ${summary.errors} ${suffix}`;
      badge.style.background = '#991B1B';
      badge.dataset.guardStatus = 'error';
      return;
    }

    const suffix = summary.warnings === 1 ? 'warning' : 'warnings';
    badge.textContent = `Style guard: ${summary.warnings} ${suffix}`;
    badge.style.background = '#B45309';
    badge.dataset.guardStatus = 'warning';
  };

  const ensureStyleGuardEnabled = () => {
    if (styleGuardState.enabled) {
      return;
    }

    window.nerdShared.enableStyleGuard(styleGuardState.defaultOptions);
  };

  const bindStyleGuardClick = () => {
    if (styleGuardState.clickBound) {
      return;
    }

    document.addEventListener(
      'click',
      (event) => {
        const target = event.target;
        if (!(target instanceof Element)) {
          return;
        }

        if (target.closest('#nerd-style-guard-panel')) {
          return;
        }

        if (!target.closest('#nerd-style-guard-badge')) {
          const panel = document.getElementById('nerd-style-guard-panel');
          if (panel?.dataset.open === 'true') {
            panel.dataset.open = 'false';
            panel.style.display = 'none';
            document.getElementById('nerd-style-guard-badge')?.removeAttribute('data-guard-panel-open');
          }

          return;
        }

        event.preventDefault();
        event.stopPropagation();
        ensureStyleGuardEnabled();
        const summary = scanStyleGuardSummary();
        updateStyleGuardBadge(summary);
        updateStyleGuardPanel(summary);
      },
      true,
    );

    styleGuardState.clickBound = true;
  };

  const clearStyleGuardHighlights = () => {
    for (const element of styleGuardState.highlighted) {
      element.style.outline = '';
      element.style.outlineOffset = '';
      element.removeAttribute('data-nerd-style-guard-hit');
      element.removeAttribute('data-nerd-style-guard-level');
    }
    styleGuardState.highlighted = [];
  };

  const pushViolation = (element, level, ratio, foreground, background, text, seen, items) => {
    if (seen.has(element)) {
      return;
    }

    seen.add(element);
    const item = { level, ratio, foreground, background, text: text.slice(0, 80) };
    items.push(item);
    element.style.outline = level === 'error' ? '2px solid #F27271' : '2px solid #F59E0B';
    element.style.outlineOffset = '2px';
    element.setAttribute('data-nerd-style-guard-hit', ratio.toFixed(2));
    element.setAttribute('data-nerd-style-guard-level', level);
    styleGuardState.highlighted.push(element);
  };

  const considerTextContrast = (element, seen, items) => {
    if (!(element instanceof HTMLElement) || seen.has(element)) {
      return;
    }

    if (element.closest('.mud-tabs-panels, .mud-tab-panel')) {
      return;
    }

    const text = (element.innerText ?? element.textContent ?? '').trim();
    if (!text || text.length > 120) {
      return;
    }

    if (element.matches('button, [role="button"], [class*="mud-button"]')) {
      return;
    }

    const style = getComputedStyle(element);
    if (style.display === 'none' || style.visibility === 'hidden' || style.opacity === '0') {
      return;
    }

    const foreground = style.color;
    const background = resolveBackgroundColor(element);
    const ratio = contrastRatio(foreground, background);
    const level = classifyContrast(element, ratio, foreground, background);
    if (!level) {
      return;
    }

    pushViolation(element, level, ratio, foreground, background, text, seen, items);
  };

  const isTransparentBackground = (color) =>
    !color || color === 'transparent' || color === 'rgba(0, 0, 0, 0)';

  const considerIntentMudControl = (element, seen, items) => {
    if (!(element instanceof HTMLElement) || seen.has(element)) {
      return;
    }

    if (!element.matches('.mud-button-outlined, .mud-chip-outlined, .mud-chip-filled')) {
      return;
    }

    const hasIntentClass = [...element.classList].some((name) =>
      /-(primary-action|secondary-action|success|danger|highlight|info)$/.test(name),
    );
    if (!hasIntentClass) {
      return;
    }

    const text = (element.innerText ?? element.textContent ?? '').trim();
    if (!text || text.length > 120) {
      return;
    }

    const style = getComputedStyle(element);
    if (style.display === 'none' || style.visibility === 'hidden' || style.opacity === '0') {
      return;
    }

    const foreground = style.color;
    const surfaceBackground = resolveBackgroundColor(element);
    const background = element.matches('.mud-chip-filled')
      ? style.backgroundColor
      : surfaceBackground;
    const ratio = contrastRatio(foreground, background);

    const brokenOutlinedPaint =
      element.matches('.mud-button-outlined, .mud-chip-outlined') &&
      isWhite(foreground) &&
      (isWhite(surfaceBackground) || isTransparentBackground(style.backgroundColor));

    const brokenFilledPaint =
      element.matches('.mud-chip-filled') &&
      (isTransparentBackground(style.backgroundColor) ||
        style.backgroundColor === 'rgba(0, 0, 0, 0.12)' ||
        (isWhite(foreground) && isWhite(surfaceBackground)));

    if (!brokenOutlinedPaint && !brokenFilledPaint) {
      return;
    }

    pushViolation(
      element,
      'error',
      ratio,
      foreground,
      background,
      text,
      seen,
      items,
    );
  };

  const considerSwitchContrast = (switchRoot, seen, items) => {
    const track = switchRoot.querySelector('.mud-switch-track');
    const thumb = switchRoot.querySelector('.mud-switch-thumb-medium, .mud-switch-thumb');
    const button = switchRoot.querySelector('.mud-button-root');
    const pageBackground = resolveBackgroundColor(switchRoot);

    const considerPart = (element, label, background) => {
      if (!(element instanceof HTMLElement)) {
        return;
      }

      const style = getComputedStyle(element);
      if (style.display === 'none' || style.visibility === 'hidden' || style.opacity === '0') {
        return;
      }

      const foreground = style.backgroundColor;
      const ratio = contrastRatio(foreground, background);
      const level = classifySurfaceContrast(ratio, foreground, background);
      if (!level) {
        return;
      }

      pushViolation(element, level, ratio, foreground, background, label, seen, items);
    };

    if (track) {
      const trackStyle = getComputedStyle(track);
      const fill = trackStyle.backgroundColor;
      const borderColor = trackStyle.borderColor;
      const borderWidth = Number.parseFloat(trackStyle.borderTopWidth) || 0;
      const fillRatio = contrastRatio(fill, pageBackground);
      const fillLevel = classifySurfaceContrast(fillRatio, fill, pageBackground);

      if (fillLevel) {
        const borderRatio = borderWidth > 0 ? contrastRatio(borderColor, pageBackground) : 0;
        const borderLevel =
          borderWidth > 0 ? classifySurfaceContrast(borderRatio, borderColor, pageBackground) : 'error';
        if (!borderLevel) {
          return;
        }

        pushViolation(track, fillLevel, fillRatio, fill, pageBackground, 'Switch track', seen, items);
      }
    }

    if (thumb) {
      const trackBackground = track
        ? getComputedStyle(track).backgroundColor
        : pageBackground;
      considerPart(thumb, 'Switch thumb', trackBackground);
    } else if (button) {
      const trackBackground = track
        ? getComputedStyle(track).backgroundColor
        : pageBackground;
      considerPart(button, 'Switch thumb', trackBackground);
    }
  };

  const scanStyleGuardSummary = () => {
    if (!styleGuardState.enabled) {
      return { errors: 0, warnings: 0, items: [] };
    }

    clearStyleGuardHighlights();
    const scopes = document.querySelectorAll(styleGuardState.scanSelector);
    const items = [];
    const seen = new Set();

    scopes.forEach((scope) => {
      const placement = scope.getAttribute('data-nerd-style-guard');
      const skipSwitchContrast = placement === 'catalog-toolbar';

      if (!skipSwitchContrast) {
        scope.querySelectorAll('.mud-switch').forEach((switchRoot) => {
          considerSwitchContrast(switchRoot, seen, items);
        });
      }

      scope.querySelectorAll(
        '.mud-tabs-tabbar .mud-tab, .mud-switch .mud-typography, .mud-input-label, label:not(.mud-switch), [role="tab"]',
      ).forEach((element) => {
        considerTextContrast(element, seen, items);
      });

      scope.querySelectorAll('.mud-button-outlined, .mud-chip-outlined, .mud-chip-filled').forEach((element) => {
        considerIntentMudControl(element, seen, items);
      });
    });

    const summary = {
      errors: items.filter((item) => item.level === 'error').length,
      warnings: items.filter((item) => item.level === 'warning').length,
      items,
    };

    if (summary.errors > 0 || summary.warnings > 0) {
      console.warn('[nerd-style-guard] contrast scan', summary);
    }

    return summary;
  };

  const scanStyleGuard = () => scanStyleGuardSummary().items;

  window.nerdShared.enableStyleGuard = (options = {}) => {
    styleGuardState.enabled = true;
    styleGuardState.errorRatio = options.errorRatio ?? options.minRatio ?? 4.5;
    styleGuardState.warningRatio = options.warningRatio ?? 3;
    styleGuardState.scanSelector = options.scanSelector ?? styleGuardState.scanSelector;
    styleGuardState.defaultOptions = {
      errorRatio: styleGuardState.errorRatio,
      warningRatio: styleGuardState.warningRatio,
      scanSelector: styleGuardState.scanSelector,
    };
    bindStyleGuardClick();

    const run = () => {
      const summary = scanStyleGuardSummary();
      updateStyleGuardBadge(summary);
    };

    window.requestAnimationFrame(run);
    window.setTimeout(run, 400);
    window.setTimeout(run, 1200);
  };

  window.nerdShared.disableStyleGuard = () => {
    styleGuardState.enabled = false;
    clearStyleGuardHighlights();
    const panel = document.getElementById('nerd-style-guard-panel');
    if (panel) {
      panel.dataset.open = 'false';
      panel.style.display = 'none';
    }
  };

  window.nerdShared.scanStyleGuard = scanStyleGuard;
  window.nerdShared.scanStyleGuardSummary = scanStyleGuardSummary;
  window.nerdShared.scanStyleGuardCount = () => scanStyleGuardSummary().errors;
  window.nerdShared.toggleStyleGuardPanel = () => {
    ensureStyleGuardEnabled();
    const summary = scanStyleGuardSummary();
    updateStyleGuardBadge(summary);
    updateStyleGuardPanel(summary);
  };

  bindStyleGuardClick();
})();
