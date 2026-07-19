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
})();
