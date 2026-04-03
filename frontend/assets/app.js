import { apiRequest } from "./http.js";
import { initTheme, getToken, clearToken, toggleTheme } from "./storage.js";
import { AUTH_TABS } from "./constants.js";
import { createAuthHandlers } from "./pages/auth.js";
import { createDashboardHandlers } from "./pages/dashboard.js";
import { closeAnalytics, openAnalytics } from "./features/analytics.js";
import { createSessionHandlers } from "./features/session.js";
import { bindAppEvents } from "./features/events.js";
import { applyLocale, initLocale, toggleLocale } from "./i18n.js";
import { collectElements, renderAppShell } from "./layout.js";

const root = document.getElementById("app");
if (!root) throw new Error("Missing #app root element");

renderAppShell(root);
initLocale();

/** @type {import("./types.js").AppState} */
const state = {
  urls: [],
  user: null,
  activeAuthTab: AUTH_TABS.LOGIN,
};

const el = collectElements();

const dashboard = createDashboardHandlers({
  state,
  el,
  apiRequest,
  openAnalytics: (urlId) => openAnalytics(el, apiRequest, urlId),
});

const session = createSessionHandlers({
  root,
  state,
  el,
  apiRequest,
  getToken,
  clearToken,
  loadUrls: () => dashboard.loadUrls(),
});

const auth = createAuthHandlers({
  root,
  state,
  el,
  apiRequest,
  loadUrls: () => dashboard.loadUrls(),
  showOnly: (view) => session.showOnly(view),
  setAuthTab: (tab) => session.setAuthTab(tab),
  renderUrls: () => dashboard.renderUrls(),
});

initTheme();
bindAppEvents({
  el,
  auth,
  dashboard,
  session,
  toggleTheme,
  onLanguageToggle: () => {
    toggleLocale();
    applyLocale(root);
    dashboard.renderUrls();
  },
  closeAnalytics: () => closeAnalytics(el),
});
applyLocale(root);
session.bootstrap();
