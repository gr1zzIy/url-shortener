import { TABLE_ACTIONS } from "../constants.js";
import { API_ORIGIN } from "../config.js";
import { getMessages } from "../i18n.js";
import { notify, readError, renderUrlsTable } from "../ui.js";

export function createDashboardHandlers(ctx) {
  const { state, el, apiRequest, openAnalytics } = ctx;

  return {
    async loadUrls() {
      const messages = getMessages();
      const result = await apiRequest("/api/urls?page=1&pageSize=100");
      state.urls = Array.isArray(result.items) ? result.items : [];
      const userEmail = state.user && state.user.email ? state.user.email : "";
      el.userMeta.textContent = userEmail ? `${messages.dashboard.signedInAsPrefix} ${userEmail}` : "";
      this.renderUrls();
    },

    renderUrls() {
      const query = el.searchInput.value.trim().toLowerCase();
      const rows = state.urls.filter((item) => {
        if (!query) return true;
        return item.shortCode.toLowerCase().includes(query) || item.originalUrl.toLowerCase().includes(query);
      });

      renderUrlsTable(el.urlsBody, rows, API_ORIGIN);
    },

    async onCreateUrl(event) {
      event.preventDefault();
      const messages = getMessages();
      const formNode = event.currentTarget;
      const form = new FormData(formNode);

      const expiresRaw = String(form.get("expiresAt") || "").trim();
      const customCode = String(form.get("customCode") || "").trim();

      try {
        await apiRequest("/api/urls", {
          method: "POST",
          body: {
            originalUrl: String(form.get("originalUrl") || "").trim(),
            customCode: customCode || null,
            expiresAt: expiresRaw ? new Date(expiresRaw).toISOString() : null,
          },
        });

        formNode.reset();
        await this.loadUrls();
        notify(el.toastRoot, messages.dashboard.shortLinkCreated);
      } catch (error) {
        notify(el.toastRoot, readError(error), true);
      }
    },

    async onTableAction(event) {
      const messages = getMessages();
      const target = event.target;
      if (!(target instanceof HTMLElement)) return;

      const action = target.dataset.action;
      if (!action) return;

      const id = target.dataset.id;

      try {
        if (action === TABLE_ACTIONS.COPY) {
          const shortCode = target.dataset.code || "";
          const value = `${API_ORIGIN}/${shortCode}`;
          await navigator.clipboard.writeText(value);
          notify(el.toastRoot, messages.dashboard.copiedToClipboard);
          return;
        }

        if (!id) return;

        if (action === TABLE_ACTIONS.STATS) {
          await openAnalytics(id);
          return;
        }

        if (action === TABLE_ACTIONS.DEACTIVATE) {
          if (!window.confirm(messages.dashboard.deactivateConfirm)) return;
          await apiRequest(`/api/urls/${id}/deactivate`, { method: "POST" });
          await this.loadUrls();
          notify(el.toastRoot, messages.dashboard.linkDeactivated);
          return;
        }

        if (action === TABLE_ACTIONS.DELETE) {
          if (!window.confirm(messages.dashboard.deleteConfirm)) return;
          await apiRequest(`/api/urls/${id}`, { method: "DELETE" });
          await this.loadUrls();
          notify(el.toastRoot, messages.dashboard.linkDeleted);
        }
      } catch (error) {
        notify(el.toastRoot, readError(error), true);
      }
    },
  };
}
