import { AUTH_TABS, VIEWS } from "../constants.js";
import { getMessages } from "../i18n.js";
import { clearToken, setToken } from "../storage.js";
import { notify, readError } from "../ui.js";

export function createAuthHandlers(ctx) {
  const { root, state, el, apiRequest, loadUrls, showOnly, setAuthTab, renderUrls } = ctx;

  return {
    async onLogin(event) {
      event.preventDefault();
      const messages = getMessages();
      const form = new FormData(event.currentTarget);

      try {
        const data = await apiRequest("/api/auth/login", {
          method: "POST",
          body: {
            email: String(form.get("email") || "").trim(),
            password: String(form.get("password") || ""),
          },
          withAuth: false,
        });

        setToken(data.accessToken);
        state.user = await apiRequest("/api/auth/me");
        showOnly(VIEWS.DASHBOARD);
        await loadUrls();
        notify(el.toastRoot, messages.auth.loggedIn);
      } catch (error) {
        notify(el.toastRoot, readError(error), true);
      }
    },

    async onRegister(event) {
      event.preventDefault();
      const messages = getMessages();
      const form = new FormData(event.currentTarget);

      try {
        const data = await apiRequest("/api/auth/register", {
          method: "POST",
          body: {
            email: String(form.get("email") || "").trim(),
            password: String(form.get("password") || ""),
          },
          withAuth: false,
        });

        setToken(data.accessToken);
        state.user = await apiRequest("/api/auth/me");
        showOnly(VIEWS.DASHBOARD);
        await loadUrls();
        notify(el.toastRoot, messages.auth.accountCreated);
      } catch (error) {
        notify(el.toastRoot, readError(error), true);
      }
    },

    async onForgotPassword(event) {
      event.preventDefault();
      const messages = getMessages();
      const form = new FormData(event.currentTarget);
      el.forgotHint.textContent = "";

      try {
        const result = await apiRequest("/api/auth/forgot-password", {
          method: "POST",
          body: {
            email: String(form.get("email") || "").trim(),
          },
          withAuth: false,
        });

        const message = result.resetUrl ? `${result.message} ${result.resetUrl}` : result.message;
        el.forgotHint.textContent = message || messages.auth.resetInstructionsSent;
        notify(el.toastRoot, messages.auth.resetInstructionsGenerated);
      } catch (error) {
        notify(el.toastRoot, readError(error), true);
      }
    },

    async onResetPassword(event) {
      event.preventDefault();
      const messages = getMessages();
      const form = new FormData(event.currentTarget);

      try {
        await apiRequest("/api/auth/reset-password", {
          method: "POST",
          body: {
            email: String(form.get("email") || "").trim(),
            token: String(form.get("token") || ""),
            newPassword: String(form.get("newPassword") || ""),
          },
          withAuth: false,
        });

        notify(el.toastRoot, messages.auth.passwordUpdated);
        if (root.dataset.forceView === VIEWS.RESET) {
          window.location.href = "/";
        } else {
          showOnly(VIEWS.AUTH);
          setAuthTab(AUTH_TABS.LOGIN);
        }
      } catch (error) {
        notify(el.toastRoot, readError(error), true);
      }
    },

    async onLogout() {
      try {
        await apiRequest("/api/auth/logout", { method: "POST" });
      } catch {
        // Logout should still clear local state even when server already invalidated the session.
      }

      clearToken();
      state.user = null;
      state.urls = [];
      renderUrls();
      showOnly(VIEWS.AUTH);
      setAuthTab(AUTH_TABS.LOGIN);
      notify(el.toastRoot, getMessages().auth.loggedOut);
    },
  };
}
