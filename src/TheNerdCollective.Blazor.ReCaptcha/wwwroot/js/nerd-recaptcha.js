let v2ScriptPromise;
let enterpriseScriptPromises = new Map();
const componentStates = new WeakMap();

export async function render(host, siteKey, theme, size, dotNetRef, enterprise, action, tokenInputName) {
  detach(host);
  const state = { disposed: false, form: null, submitHandler: null, inFlight: false, replaying: false };
  componentStates.set(host, state);

  if (enterprise) {
    await ensureEnterpriseScript(siteKey);
    await waitForEnterprise();
    if (state.disposed) return;

    const form = host.closest("form");
    if (form) {
      const submitHandler = async (event) => {
        if (state.replaying) {
          state.replaying = false;
          return;
        }

        event.preventDefault();
        event.stopImmediatePropagation();
        if (state.inFlight || state.disposed) return;
        state.inFlight = true;
        const submitter = event.submitter;
        const submitterWasDisabled = submitter?.disabled ?? false;
        if (submitter) submitter.disabled = true;

        try {
          const token = await executeToken(siteKey, action);
          if (state.disposed) {
            if (submitter) submitter.disabled = submitterWasDisabled;
            return;
          }
          const tokenInput = Array.from(form.elements)
            .find((element) => element.name === tokenInputName);
          if (!token || !tokenInput) {
            throw new Error("reCAPTCHA Enterprise did not return a token");
          }
          tokenInput.value = token;
          await dotNetRef.invokeMethodAsync("OnGoogleTokenAsync", token);
          if (state.disposed) {
            if (submitter) submitter.disabled = submitterWasDisabled;
            return;
          }

          if (submitter) submitter.disabled = submitterWasDisabled;
          state.replaying = true;
          form.requestSubmit(submitterWasDisabled ? undefined : submitter);
          queueMicrotask(() => {
            state.replaying = false;
            state.inFlight = false;
          });
        } catch {
          state.inFlight = false;
          if (submitter) submitter.disabled = submitterWasDisabled;
          if (!state.disposed) {
            await dotNetRef.invokeMethodAsync("OnGoogleErrorAsync");
          }
        }
      };

      state.form = form;
      state.submitHandler = submitHandler;
      form.addEventListener("submit", submitHandler, true);
    }
    return;
  }

  await ensureV2Script();
  await waitForGrecaptcha();
  if (state.disposed) return;
  window.grecaptcha.render(host, {
    sitekey: siteKey,
    theme: theme || "light",
    size: size || "normal",
    callback: (token) => dotNetRef.invokeMethodAsync("OnGoogleTokenAsync", token),
    "expired-callback": () => dotNetRef.invokeMethodAsync("OnGoogleExpiredAsync"),
    "error-callback": () => dotNetRef.invokeMethodAsync("OnGoogleErrorAsync")
  });
}

export async function execute(host, siteKey, action) {
  const state = componentStates.get(host);
  if (!state || state.disposed) {
    throw new Error("reCAPTCHA component is not active");
  }

  await waitForEnterprise();
  if (state.disposed) {
    throw new Error("reCAPTCHA component was disposed");
  }

  return executeToken(siteKey, action);
}

export function detach(host) {
  const state = componentStates.get(host);
  if (!state) return;

  state.disposed = true;
  if (state.form && state.submitHandler) {
    state.form.removeEventListener("submit", state.submitHandler, true);
  }
  componentStates.delete(host);
}

async function executeToken(siteKey, action) {
  return window.grecaptcha.enterprise.execute(siteKey, { action });
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
