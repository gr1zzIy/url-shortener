import { byId } from "./ui.js";

export function renderAppShell(root) {
  root.innerHTML = `
    <div class="app">
      <header class="topbar">
        <div>
          <p class="brand" data-i18n="layout.brand">GlassLink</p>
          <p class="muted help" data-i18n="layout.tagline">Vanilla HTML/CSS/JS frontend</p>
        </div>
        <div class="topbar-actions">
          <button class="btn" id="themeToggle" type="button" data-i18n="layout.themeToggle">Theme</button>
          <button class="btn" id="languageToggle" type="button" data-i18n="layout.languageToggle">UA</button>
          <button class="btn hidden" id="logoutButton" type="button" data-i18n="layout.logout">Logout</button>
        </div>
      </header>

      <section class="card section" id="authView">
        <div class="row wrap">
          <button class="btn primary" type="button" data-auth-tab="login" data-i18n="auth.loginTab">Login</button>
          <button class="btn" type="button" data-auth-tab="register" data-i18n="auth.registerTab">Register</button>
          <button class="btn" type="button" data-auth-tab="forgot" data-i18n="auth.forgotTab">Forgot password</button>
        </div>

        <form class="stack" id="loginForm" style="margin-top: 1rem;">
          <input class="input" type="email" name="email" placeholder="Email" data-i18n-placeholder="auth.emailPlaceholder" required />
          <input class="input" type="password" name="password" placeholder="Password" data-i18n-placeholder="auth.passwordPlaceholder" required />
          <button class="btn primary" type="submit" data-i18n="auth.signIn">Sign in</button>
        </form>

        <form class="stack hidden" id="registerForm" style="margin-top: 1rem;">
          <input class="input" type="email" name="email" placeholder="Email" data-i18n-placeholder="auth.emailPlaceholder" required />
          <input class="input" type="password" name="password" placeholder="Password" data-i18n-placeholder="auth.passwordPlaceholder" minlength="6" required />
          <button class="btn primary" type="submit" data-i18n="auth.createAccount">Create account</button>
        </form>

        <form class="stack hidden" id="forgotForm" style="margin-top: 1rem;">
          <input class="input" type="email" name="email" placeholder="Email" data-i18n-placeholder="auth.emailPlaceholder" required />
          <button class="btn primary" type="submit" data-i18n="auth.sendResetLink">Send reset link</button>
          <p class="muted help" id="forgotHint"></p>
        </form>
      </section>

      <section class="card section hidden" id="resetView">
        <h2 data-i18n="auth.resetTitle">Reset password</h2>
        <p class="muted help" style="margin-top: 0.3rem;" data-i18n="layout.resetHint">Use the link you received from the API.</p>
        <form class="stack" id="resetForm" style="margin-top: 1rem;">
          <input class="input" type="email" name="email" placeholder="Email" data-i18n-placeholder="auth.emailPlaceholder" required />
          <input class="input" type="text" name="token" placeholder="Reset token" data-i18n-placeholder="auth.resetTokenPlaceholder" required />
          <input class="input" type="password" name="newPassword" placeholder="New password" data-i18n-placeholder="auth.newPasswordPlaceholder" minlength="6" required />
          <button class="btn primary" type="submit" data-i18n="auth.changePassword">Change password</button>
        </form>
      </section>

      <section class="card section hidden" id="dashboardView">
        <div class="row between wrap">
          <div>
            <h2 data-i18n="layout.yourLinks">Your links</h2>
            <p class="muted help" id="userMeta"></p>
          </div>
          <input class="input" style="max-width: 260px;" id="searchInput" data-i18n-placeholder="dashboard.searchPlaceholder" placeholder="Search by code or URL" />
        </div>

        <div class="card section" style="margin-top: 1rem;">
          <h3 data-i18n="layout.createShortUrl">Create short URL</h3>
          <form class="grid cols-2" id="createForm" style="margin-top: 0.7rem;">
            <input class="input" type="url" name="originalUrl" placeholder="https://example.com" data-i18n-placeholder="dashboard.originalUrlPlaceholder" required />
            <input class="input" type="text" name="customCode" data-i18n-placeholder="dashboard.customCodePlaceholder" placeholder="Custom code (optional)" />
            <input class="input" type="datetime-local" name="expiresAt" />
            <button class="btn primary" type="submit" data-i18n="dashboard.createButton">Create</button>
          </form>
        </div>

        <div class="table-wrap" style="margin-top: 1rem;">
          <table>
            <thead>
              <tr>
                <th data-i18n="dashboard.codeHeader">Code</th>
                <th data-i18n="dashboard.originalUrlHeader">Original URL</th>
                <th data-i18n="dashboard.clicksHeader">Clicks</th>
                <th data-i18n="dashboard.statusHeader">Status</th>
                <th data-i18n="dashboard.actionsHeader">Actions</th>
              </tr>
            </thead>
            <tbody id="urlsBody"></tbody>
          </table>
        </div>
      </section>

      <div class="overlay hidden" id="analyticsOverlay">
        <div class="card section modal">
          <div class="row between">
            <h3 data-i18n="actions.analytics">Analytics</h3>
            <button class="btn" id="closeAnalytics" type="button" data-i18n="actions.close">Close</button>
          </div>
          <div id="analyticsBody" style="margin-top: 1rem;"></div>
        </div>
      </div>

      <div class="toast-root" id="toastRoot"></div>
    </div>
  `;
}

export function collectElements() {
  return {
    authView: byId("authView"),
    dashboardView: byId("dashboardView"),
    resetView: byId("resetView"),
    loginForm: byId("loginForm"),
    registerForm: byId("registerForm"),
    forgotForm: byId("forgotForm"),
    forgotHint: byId("forgotHint"),
    resetForm: byId("resetForm"),
    createForm: byId("createForm"),
    urlsBody: byId("urlsBody"),
    userMeta: byId("userMeta"),
    searchInput: byId("searchInput"),
    themeToggle: byId("themeToggle"),
    languageToggle: byId("languageToggle"),
    logoutButton: byId("logoutButton"),
    analyticsOverlay: byId("analyticsOverlay"),
    analyticsBody: byId("analyticsBody"),
    closeAnalytics: byId("closeAnalytics"),
    toastRoot: byId("toastRoot"),
    authTabs: document.querySelectorAll("[data-auth-tab]"),
  };
}

