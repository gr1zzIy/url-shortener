import { authStore } from "../auth/authStore";
import type { AuthResponse, ProblemDetails } from "./contracts";

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL as string) || "";

export class ApiError extends Error {
    public readonly status: number;
    public readonly problem?: ProblemDetails;

    constructor(status: number, message: string, problem?: ProblemDetails) {
        super(message);
        this.status = status;
        this.problem = problem;
    }
}

type HttpMethod = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";

async function parseProblemDetails(res: Response): Promise<ProblemDetails | undefined> {
    const ct = res.headers.get("content-type") || "";
    if (!ct.includes("application/problem+json")) return undefined;
    try {
        return (await res.json()) as ProblemDetails;
    } catch {
        return undefined;
    }
}

async function rawFetch(method: HttpMethod, path: string, body?: unknown): Promise<Response> {
    const url = `${API_BASE_URL}${path}`;

    const headers: Record<string, string> = { Accept: "application/json" };

    const token = authStore.getAccessToken();
    if (token) headers["Authorization"] = `Bearer ${token}`;
    if (body !== undefined) headers["Content-Type"] = "application/json";

    return fetch(url, {
        method,
        headers,
        credentials: "include", // REQUIRED for refresh cookie
        body: body !== undefined ? JSON.stringify(body) : undefined,
    });
}

async function refreshAccessToken(): Promise<boolean> {
    try {
        const res = await rawFetch("POST", "/api/auth/refresh");
        if (!res.ok) return false;
        const data = (await res.json()) as AuthResponse;
        authStore.setAccessToken(data.accessToken);
        return true;
    } catch {
        return false;
    }
}

async function request<T>(method: HttpMethod, path: string, body?: unknown, retry = true): Promise<T> {
    const res = await rawFetch(method, path, body);

    if (res.status === 401 && retry) {
        const ok = await refreshAccessToken();
        if (ok) return request<T>(method, path, body, false);
    }

    if (!res.ok) {
        const problem = await parseProblemDetails(res);
        const msg = problem?.title || `Request failed (${res.status})`;
        throw new ApiError(res.status, msg, problem);
    }

    if (res.status === 204) return undefined as T;

    const ct = res.headers.get("content-type") || "";
    if (ct.includes("application/json")) return (await res.json()) as T;

    return undefined as T;
}

export const http = {
    get: <T>(path: string) => request<T>("GET", path),
    post: <T>(path: string, body?: unknown) => request<T>("POST", path, body),
    del: <T>(path: string) => request<T>("DELETE", path),
};
