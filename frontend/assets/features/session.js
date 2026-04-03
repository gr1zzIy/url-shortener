import { AUTH_TABS, VIEWS } from "../constants.js";

/** @returns {import("../types.js").SessionHandlers} */
export function createSessionHandlers(ctx) {
  const { root, state, el, apiRequest, getToken, clearToken, loadUrls } = ctx;

  return {
    async bootstrap() {
      const forcedView = root.dataset.forceView;
      const params = new URLSearchParams(window.location.search);

      if (forcedView === VIEWS.RESET) {
        this.showOnly(VIEWS.RESET);
        this.fillResetFields(params);
        return;
      }

      const token = getToken();
      if (!token) {
        this.showOnly(VIEWS.AUTH);
        this.setAuthTab(params.get("tab") || AUTH_TABS.LOGIN);
        return;
      }

      try {
        state.user = await apiRequest("/api/auth/me");
        this.showOnly(VIEWS.DASHBOARD);
        await loadUrls();
      } catch {
        clearToken();
        this.showOnly(VIEWS.AUTH);
      }
    },

    showOnly(view) {
      el.authView.classList.toggle("hidden", view !== VIEWS.AUTH);
      el.dashboardView.classList.toggle("hidden", view !== VIEWS.DASHBOARD);
      el.resetView.classList.toggle("hidden", view !== VIEWS.RESET);
      el.logoutButton.classList.toggle("hidden", view !== VIEWS.DASHBOARD);
    },

    setAuthTab(tab) {
      state.activeAuthTab = tab;

      const mapping = {
        [AUTH_TABS.LOGIN]: el.loginForm,
        [AUTH_TABS.REGISTER]: el.registerForm,
        [AUTH_TABS.FORGOT]: el.forgotForm,
      };

      for (const [key, form] of Object.entries(mapping)) {
        form.classList.toggle("hidden", key !== tab);
      }

      for (const button of el.authTabs) {
        button.classList.toggle("primary", button.dataset.authTab === tab);
      }
    },

    fillResetFields(params) {
      const email = params.get("email") || "";
      const token = params.get("token") || "";
      el.resetForm.elements.email.value = email;
      el.resetForm.elements.token.value = token;
    },
  };
}
