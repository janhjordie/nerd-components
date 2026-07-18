window.nerdShared = window.nerdShared || {
  copyText: async (text) => {
    if (navigator.clipboard && navigator.clipboard.writeText) {
      await navigator.clipboard.writeText(text);
      return;
    }

    const textarea = document.createElement('textarea');
    textarea.value = text;
    document.body.appendChild(textarea);
    textarea.select();
    document.execCommand('copy');
    document.body.removeChild(textarea);
  },
  downloadText: (fileName, text) => {
    const blob = new Blob([text], { type: 'text/plain;charset=utf-8' });
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
  const copyTokenToPopovers = (root) => {
    if (!activeToken) {
      return;
    }

    const popovers = [];
    if (root.matches?.('.mud-popover, .mud-picker-popover')) {
      popovers.push(root);
    }
    root.querySelectorAll?.('.mud-popover, .mud-picker-popover').forEach((popover) => {
      popovers.push(popover);
    });
    popovers.forEach((popover) => {
      popover.classList.add(activeToken);
    });
  };

  document.addEventListener('pointerdown', (event) => {
    const tokenHost = event.target.closest?.('[data-nerd-token]');
    if (tokenHost) {
      activeToken = tokenHost.dataset.nerdToken;
    }
  }, true);

  document.addEventListener('focusin', (event) => {
    const tokenHost = event.target.closest?.('[data-nerd-token]');
    if (tokenHost) {
      activeToken = tokenHost.dataset.nerdToken;
    }
  }, true);

  const observer = new MutationObserver((mutations) => {
    mutations.forEach((mutation) => {
      if (mutation.type === 'attributes') {
        copyTokenToPopovers(mutation.target);
      }
      mutation.addedNodes.forEach((node) => {
        if (node.nodeType === Node.ELEMENT_NODE) {
          copyTokenToPopovers(node);
        }
      });
    });
  });

  observer.observe(document.body, {
    childList: true,
    subtree: true,
    attributes: true,
    attributeFilter: ['class']
  });
  window.nerdShared.portalTokenObserver = observer;
})();
