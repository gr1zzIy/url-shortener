import { getMessages } from "./i18n.js";

export function byId(id) {
  const node = document.getElementById(id);
  if (!node) throw new Error(`Missing element #${id}`);
  return node;
}

export function notify(toastRoot, message, isError = false) {
  const toast = document.createElement("div");
  toast.className = "toast";

  if (isError) {
    toast.style.borderColor = "var(--danger)";
  }

  toast.textContent = message;
  toastRoot.appendChild(toast);

  window.setTimeout(() => {
    toast.remove();
  }, 3200);
}

export function readError(error) {
  const messages = getMessages();
  if (error && typeof error === "object" && "message" in error) {
    return String(error.message || messages.common.requestFailed);
  }

  return messages.common.requestFailed;
}

export function formatUtc(value) {
  if (!value) return "-";

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";

  return `${date.toLocaleDateString()} ${date.toLocaleTimeString()}`;
}

export function escapeHtml(value) {
  return String(value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#39;");
}

export function escapeAttr(value) {
  return escapeHtml(value).replaceAll("`", "&#96;");
}

export function renderUrlsTable(container, rows, apiOrigin) {
  const messages = getMessages();

  if (rows.length === 0) {
    container.innerHTML = `<tr><td colspan="5" class="muted">${messages.dashboard.noLinks}</td></tr>`;
    return;
  }

  container.innerHTML = rows
    .map((item) => {
      const shortLink = `${apiOrigin}/${item.shortCode}`;
      const statusClass = item.isActive ? "ok" : "off";
      const statusText = item.isActive ? messages.dashboard.statusActive : messages.dashboard.statusInactive;

      return `
        <tr>
          <td class="code">/${escapeHtml(item.shortCode)}</td>
          <td>
            <div><a href="${escapeAttr(item.originalUrl)}" target="_blank" rel="noreferrer">${escapeHtml(item.originalUrl)}</a></div>
            <div class="muted help code">${escapeHtml(shortLink)}</div>
          </td>
          <td>${item.clicks}</td>
          <td><span class="badge ${statusClass}">${statusText}</span></td>
          <td>
            <div class="row wrap">
              <button class="btn" data-action="copy" data-code="${escapeAttr(item.shortCode)}">${messages.actions.copy}</button>
              <button class="btn" data-action="stats" data-id="${item.id}">${messages.actions.analytics}</button>
              <button class="btn" data-action="deactivate" data-id="${item.id}" ${item.isActive ? "" : "disabled"}>${messages.actions.deactivate}</button>
              <button class="btn danger" data-action="delete" data-id="${item.id}">${messages.actions.delete}</button>
            </div>
          </td>
        </tr>
      `;
    })
    .join("");
}

export function renderAnalyticsContent(container, stats, breakdown, clicks) {
  const messages = getMessages();

  container.innerHTML = `
    <div class="analytics-grid">
      <div class="card section">
        <p class="muted help">${messages.analytics.totalClicks}</p>
        <h2>${stats.totalClicks}</h2>
      </div>
      <div class="card section">
        <p class="muted help">${messages.analytics.uniqueVisitors}</p>
        <h2>${stats.uniqueVisitors}</h2>
      </div>
    </div>
    <div class="card section" style="margin-top: 0.9rem;">
      <h3>${messages.analytics.clicksTimeline}</h3>
      ${renderSeries(stats.series)}
    </div>
    <div class="analytics-grid" style="margin-top: 0.9rem;">
      <div class="card section"><h3>${messages.analytics.countries}</h3>${renderBreakdown(breakdown.countries)}</div>
      <div class="card section"><h3>${messages.analytics.devices}</h3>${renderBreakdown(breakdown.devices)}</div>
      <div class="card section"><h3>${messages.analytics.browsers}</h3>${renderBreakdown(breakdown.browsers)}</div>
      <div class="card section"><h3>${messages.analytics.os}</h3>${renderBreakdown(breakdown.os)}</div>
    </div>
    <div class="card section" style="margin-top: 0.9rem;">
      <h3>${messages.analytics.recentClicks}</h3>
      ${renderRecentClicks(clicks)}
    </div>
  `;
}

function renderSeries(series) {
  if (!Array.isArray(series) || series.length === 0) {
    return `<p class="muted" style="margin-top: 0.6rem;">${getMessages().common.noData}</p>`;
  }

  const max = Math.max(...series.map((point) => point.clicks), 1);
  return `
    <div class="stack" style="margin-top: 0.7rem;">
      ${series
        .map((point) => {
          const percent = Math.round((point.clicks / max) * 100);
          return `
            <div class="stack" style="gap: 0.2rem;">
              <div class="row between">
                <span class="help code">${escapeHtml(point.date)}</span>
                <span class="help">${point.clicks} ${getMessages().analytics.clicksSuffix}</span>
              </div>
              <div class="bar" style="width:${percent}%;"></div>
            </div>
          `;
        })
        .join("")}
    </div>
  `;
}

function renderBreakdown(items) {
  if (!Array.isArray(items) || items.length === 0) {
    return `<p class="muted" style="margin-top: 0.6rem;">${getMessages().common.noData}</p>`;
  }

  const max = Math.max(...items.map((item) => item.count), 1);
  return `
    <div class="stack" style="margin-top: 0.7rem;">
      ${items
        .slice(0, 8)
        .map((item) => {
          const percent = Math.round((item.count / max) * 100);
          return `
            <div class="stack" style="gap: 0.2rem;">
              <div class="row between">
                <span>${escapeHtml(item.key || getMessages().common.unknown)}</span>
                <span class="help">${item.count}</span>
              </div>
              <div class="bar" style="width:${percent}%;"></div>
            </div>
          `;
        })
        .join("")}
    </div>
  `;
}

function renderRecentClicks(items) {
  if (!Array.isArray(items) || items.length === 0) {
    return `<p class="muted" style="margin-top: 0.6rem;">${getMessages().common.noData}</p>`;
  }

  return `
    <div class="table-wrap" style="margin-top: 0.7rem;">
      <table>
        <thead>
          <tr>
            <th>${getMessages().analytics.columns.time}</th>
            <th>${getMessages().analytics.columns.country}</th>
            <th>${getMessages().analytics.columns.device}</th>
            <th>${getMessages().analytics.columns.browser}</th>
            <th>${getMessages().analytics.columns.os}</th>
          </tr>
        </thead>
        <tbody>
          ${items
            .map(
              (item) => `
                <tr>
                  <td class="help">${escapeHtml(formatUtc(item.occurredAt))}</td>
                  <td>${escapeHtml(item.countryCode || getMessages().common.emptyValue)}</td>
                  <td>${escapeHtml(item.deviceType || getMessages().common.emptyValue)}</td>
                  <td>${escapeHtml(item.browser || getMessages().common.emptyValue)}</td>
                  <td>${escapeHtml(item.os || getMessages().common.emptyValue)}</td>
                </tr>
              `
            )
            .join("")}
        </tbody>
      </table>
    </div>
  `;
}

