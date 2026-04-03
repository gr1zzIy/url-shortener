import { AUTH_TABS } from "../constants.js";

/**
 * @param {{
 *   el: Record<string, any>,
 *   auth: Record<string, Function>,
 *   dashboard: Record<string, Function>,
 *   session: import("../types.js").SessionHandlers,
 *   toggleTheme: () => void,
 *   onLanguageToggle: () => void,
 *   closeAnalytics: () => void
 * }} ctx
 */
export function bindAppEvents(ctx) {
  const { el, auth, dashboard, session, toggleTheme, onLanguageToggle, closeAnalytics } = ctx;

  el.themeToggle.addEventListener("click", toggleTheme);
  el.languageToggle.addEventListener("click", onLanguageToggle);
  el.logoutButton.addEventListener("click", auth.onLogout);
  el.loginForm.addEventListener("submit", auth.onLogin);
  el.registerForm.addEventListener("submit", auth.onRegister);
  el.forgotForm.addEventListener("submit", auth.onForgotPassword);
  el.resetForm.addEventListener("submit", auth.onResetPassword);
  el.createForm.addEventListener("submit", (event) => dashboard.onCreateUrl(event));
  el.searchInput.addEventListener("input", () => dashboard.renderUrls());
  el.urlsBody.addEventListener("click", (event) => dashboard.onTableAction(event));
  el.closeAnalytics.addEventListener("click", () => closeAnalytics());
  el.analyticsOverlay.addEventListener("click", (event) => {
    if (event.target === el.analyticsOverlay) closeAnalytics();
  });

  for (const button of el.authTabs) {
    button.addEventListener("click", () => session.setAuthTab(button.dataset.authTab || AUTH_TABS.LOGIN));
  }
}

