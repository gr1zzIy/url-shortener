export const API_BASE_URL = (window.APP_CONFIG && window.APP_CONFIG.apiBaseUrl) || "http://localhost:5000";
export const API_ORIGIN = new URL(API_BASE_URL).origin;

