import { getLanguage, initLanguage as initStoredLanguage, toggleLanguage as toggleStoredLanguage } from "./storage.js";
import { MESSAGES as EN_MESSAGES } from "./messages.en.js";
import { MESSAGES as UK_MESSAGES } from "./messages.uk.js";

const DICTS = {
  en: EN_MESSAGES,
  uk: UK_MESSAGES,
};

export function initLocale() {
  return initStoredLanguage();
}

export function getLocale() {
  return getLanguage();
}


export function toggleLocale() {
  return toggleStoredLanguage();
}

export function getMessages(locale = getLocale()) {
  return DICTS[locale] || DICTS.en;
}

export function t(path, locale = getLocale()) {
  const messages = getMessages(locale);
  return path.split(".").reduce((value, key) => (value && key in value ? value[key] : undefined), messages) ?? path;
}

export function applyLocale(root) {
  const locale = getLocale();
  const messages = getMessages(locale);

  document.documentElement.lang = locale;
  document.title = root?.dataset.forceView === "reset" ? messages.titles.reset : messages.titles.main;

  document.querySelectorAll("[data-i18n]").forEach((node) => {
    const key = node.dataset.i18n;
    if (!key) return;
    node.textContent = t(key, locale);
  });

  document.querySelectorAll("[data-i18n-placeholder]").forEach((node) => {
    const key = node.dataset.i18nPlaceholder;
    if (!key) return;
    node.setAttribute("placeholder", t(key, locale));
  });
}

