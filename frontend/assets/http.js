import { API_BASE_URL } from "./config.js";
import { clearToken, getToken, setToken } from "./storage.js";

async function refreshToken() {
  try {
    const result = await apiRequest("/api/auth/refresh", {
      method: "POST",
      withAuth: false,
      retryOn401: false,
    });

    if (!result || !result.accessToken) {
      clearToken();
      return false;
    }

    setToken(result.accessToken);
    return true;
  } catch {
    clearToken();
    return false;
  }
}

export async function apiRequest(path, options = {}) {
  const method = options.method || "GET";
  const withAuth = options.withAuth !== false;
  const retryOn401 = options.retryOn401 !== false;

  const headers = { Accept: "application/json" };
  if (options.body !== undefined) {
    headers["Content-Type"] = "application/json";
  }

  if (withAuth) {
    const token = getToken();
    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method,
    headers,
    credentials: "include",
    body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
  });

  if (response.status === 401 && retryOn401 && withAuth) {
    const recovered = await refreshToken();
    if (recovered) {
      return apiRequest(path, { ...options, retryOn401: false });
    }
  }

  if (!response.ok) {
    let message = `Request failed (${response.status})`;
    const contentType = response.headers.get("content-type") || "";

    if (contentType.includes("application/json") || contentType.includes("application/problem+json")) {
      try {
        const payload = await response.json();
        message = payload.title || payload.detail || message;
      } catch {
        // Keep default message when body cannot be parsed.
      }
    }

    throw new Error(message);
  }

  if (response.status === 204) {
    return null;
  }

  const responseType = response.headers.get("content-type") || "";
  if (responseType.includes("application/json")) {
    return response.json();
  }

  return response.text();
}

