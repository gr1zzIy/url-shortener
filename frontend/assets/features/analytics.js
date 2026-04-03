import { getMessages } from "../i18n.js";
import { escapeHtml, readError, renderAnalyticsContent } from "../ui.js";

export async function openAnalytics(el, apiRequest, urlId) {
  const messages = getMessages();
  el.analyticsOverlay.classList.remove("hidden");
  el.analyticsBody.innerHTML = `<p class="muted">${messages.analytics.loading}</p>`;

  try {
    const [stats, breakdown, clicks] = await Promise.all([
      apiRequest(`/api/urls/${urlId}/stats`),
      apiRequest(`/api/urls/${urlId}/breakdown`),
      apiRequest(`/api/urls/${urlId}/clicks?take=25`),
    ]);

    renderAnalyticsContent(el.analyticsBody, stats, breakdown, clicks);
  } catch (error) {
    el.analyticsBody.innerHTML = `<p class="muted">${escapeHtml(readError(error))}</p>`;
  }
}

export function closeAnalytics(el) {
  el.analyticsOverlay.classList.add("hidden");
  el.analyticsBody.innerHTML = "";
}

