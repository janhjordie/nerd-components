/**
 * Session monitor client identity.
 * Runs synchronously before Blazor so every tab on this origin shares one id.
 * If storage is unavailable and no cookie exists, no id is invented.
 */
(() => {
  "use strict";

  const config = window.sessionMonitorClientConfig || {};
  const cookieName = config.cookieName || ".bs-sm-client";
  const storageKey = config.storageKey || "bs-sm-client";

  function readCookie(name) {
    const prefix = `${name}=`;
    const parts = document.cookie ? document.cookie.split("; ") : [];
    for (const part of parts) {
      if (part.startsWith(prefix)) {
        return decodeURIComponent(part.slice(prefix.length));
      }
    }

    return "";
  }

  function writeCookie(name, value) {
    const secure = window.location.protocol === "https:" ? "; Secure" : "";
    document.cookie = `${name}=${encodeURIComponent(value)}; Path=/; Max-Age=31536000; SameSite=Lax${secure}`;
  }

  function expireCookie(name, path) {
    document.cookie = `${name}=; Path=${path}; Max-Age=0; SameSite=Lax`;
  }

  function createId() {
    if (window.crypto && typeof window.crypto.randomUUID === "function") {
      return window.crypto.randomUUID().replace(/-/g, "");
    }

    return "xxxxxxxxxxxx4xxxyxxxxxxxxxxxxxxx".replace(/[xy]/g, (character) => {
      const random = (Math.random() * 16) | 0;
      const value = character === "x" ? random : (random & 0x3) | 0x8;
      return value.toString(16);
    });
  }

  let storageId = "";
  let storageAvailable = false;
  try {
    const probe = "__bs_sm_probe";
    window.localStorage.setItem(probe, "1");
    window.localStorage.removeItem(probe);
    storageAvailable = true;
    storageId = window.localStorage.getItem(storageKey) || "";
  } catch {
    storageAvailable = false;
  }

  const cookieId = readCookie(cookieName);
  let clientId = storageId || cookieId;
  if (!clientId && storageAvailable) {
    clientId = createId();
  }

  if (!clientId) {
    return;
  }

  if (storageAvailable) {
    try {
      window.localStorage.setItem(storageKey, clientId);
      clientId = window.localStorage.getItem(storageKey) || clientId;
    } catch {
      if (!cookieId) {
        return;
      }

      clientId = cookieId;
    }
  }

  const segments = window.location.pathname.split("/").filter(Boolean);
  let path = "";
  expireCookie(cookieName, "/");
  for (const segment of segments) {
    path += `/${segment}`;
    expireCookie(cookieName, path);
  }

  writeCookie(cookieName, clientId);
})();
