export async function render(host, siteKey, theme, size, dotNetRef) {
  await ensureScript();
  await waitForGrecaptcha();
  window.grecaptcha.render(host, {
    sitekey: siteKey,
    theme: theme || "light",
    size: size || "normal",
    callback: (token) => dotNetRef.invokeMethodAsync("OnGoogleTokenAsync", token),
    "expired-callback": () => dotNetRef.invokeMethodAsync("OnGoogleExpiredAsync"),
    "error-callback": () => dotNetRef.invokeMethodAsync("OnGoogleExpiredAsync")
  });
}

function ensureScript() {
  if (window.grecaptcha) {
    return Promise.resolve();
  }

  const existing = document.querySelector('script[data-nerd-recaptcha="1"]');
  if (existing) {
    return new Promise((resolve, reject) => {
      existing.addEventListener("load", () => resolve(), { once: true });
      existing.addEventListener("error", () => reject(new Error("reCAPTCHA script failed")), { once: true });
    });
  }

  return new Promise((resolve, reject) => {
    const script = document.createElement("script");
    script.src = "https://www.google.com/recaptcha/api.js?render=explicit";
    script.async = true;
    script.defer = true;
    script.dataset.nerdRecaptcha = "1";
    script.onload = () => resolve();
    script.onerror = () => reject(new Error("reCAPTCHA script failed"));
    document.head.appendChild(script);
  });
}

function waitForGrecaptcha(timeoutMs = 10000) {
  return new Promise((resolve, reject) => {
    const started = Date.now();
    const tick = () => {
      if (window.grecaptcha && typeof window.grecaptcha.render === "function") {
        window.grecaptcha.ready(() => resolve());
        return;
      }
      if (Date.now() - started > timeoutMs) {
        reject(new Error("reCAPTCHA not ready"));
        return;
      }
      setTimeout(tick, 50);
    };
    tick();
  });
}
