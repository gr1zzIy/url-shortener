const TOKEN_KEY = "accessToken";
const THEME_KEY = "theme";
const LANGUAGE_KEY = "language";

export function getToken() {
  return sessionStorage.getItem(TOKEN_KEY);
}

export function setToken(token) {
  sessionStorage.setItem(TOKEN_KEY, token);
}

export function clearToken() {
  sessionStorage.removeItem(TOKEN_KEY);
}

export function initTheme() {
  const stored = localStorage.getItem(THEME_KEY);
  const preferred = stored || (window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light");
  applyTheme(preferred);
}

export function toggleTheme() {
  const current = document.documentElement.dataset.theme === "dark" ? "dark" : "light";
  applyTheme(current === "dark" ? "light" : "dark");
}

export function applyTheme(theme) {
  document.documentElement.dataset.theme = theme;
  localStorage.setItem(THEME_KEY, theme);
}

export function getLanguage() {
  return localStorage.getItem(LANGUAGE_KEY);
}

export function setLanguage(language) {
  localStorage.setItem(LANGUAGE_KEY, language);
}

export function initLanguage() {
  const stored = getLanguage();
  const preferred = stored || (window.navigator.language?.toLowerCase().startsWith("uk") ? "uk" : "en");
  setLanguage(preferred === "uk" ? "uk" : "en");
  return preferred === "uk" ? "uk" : "en";
}

export function toggleLanguage() {
  const next = getLanguage() === "uk" ? "en" : "uk";
  setLanguage(next);
  return next;
}

