let v2ScriptPromise;
let enterpriseScriptPromises = new Map();

export async function render(host, siteKey, theme, size, dotNetRef, enterprise, action, tokenInputName) {
  if (enterprise) {
    await ensureEnterpriseScript(siteKey);
    await waitForEnterprise();
    const form = host.closest("form");
    if (!form) {
      throw new Error("reCAPTCHA Enterprise must be rendered inside a form");
    }

    let inFlight = false;
    form.addEventListener("submit", async (event) => {
      event.preventDefault();
      if (inFlight) return;
      inFlight = true;
      const submitter = event.submitter;
      if (submitter) submitter.disabled = true;
      try {
        const token = await window.grecaptcha.enterprise.execute(siteKey, { action });
        const tokenInput = Array.from(form.elements)
          .find((element) => element.name === tokenInputName);
        if (!token || !tokenInput) {
          throw new Error("reCAPTCHA Enterprise did not return a token");
        }
        tokenInput.value = token;
        await dotNetRef.invokeMethodAsync("OnGoogleTokenAsync", token);
        HTMLFormElement.prototype.submit.call(form);
      } catch {
        inFlight = false;
        if (submitter) submitter.disabled = false;
        await dotNetRef.invokeMethodAsync("OnGoogleErrorAsync");
      }
    }, true);
    return;
  }

  await ensureV2Script();
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

function ensureV2Script() {
  if (window.grecaptcha) {
    return Promise.resolve();
  }

  if (!v2ScriptPromise) {
    v2ScriptPromise = new Promise((resolve, reject) => {
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

  return v2ScriptPromise;
}

function ensureEnterpriseScript(siteKey) {
  if (window.grecaptcha?.enterprise) {
    return Promise.resolve();
  }

  if (!enterpriseScriptPromises.has(siteKey)) {
    const scriptPromise = new Promise((resolve, reject) => {
      const script = document.createElement("script");
      script.src = `https://www.google.com/recaptcha/enterprise.js?render=${encodeURIComponent(siteKey)}`;
      script.async = true;
      script.defer = true;
      script.dataset.nerdRecaptchaEnterprise = "1";
      script.onload = () => resolve();
      script.onerror = () => reject(new Error("reCAPTCHA Enterprise script failed"));
      document.head.appendChild(script);
    });
    enterpriseScriptPromises.set(siteKey, scriptPromise);
  }

  return enterpriseScriptPromises.get(siteKey);
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

function waitForEnterprise(timeoutMs = 10000) {
  return new Promise((resolve, reject) => {
    const started = Date.now();
    const tick = () => {
      if (window.grecaptcha?.enterprise?.execute) {
        window.grecaptcha.enterprise.ready(() => resolve());
        return;
      }
      if (Date.now() - started > timeoutMs) {
        reject(new Error("reCAPTCHA Enterprise not ready"));
        return;
      }
      setTimeout(tick, 50);
    };
    tick();
  });
}
