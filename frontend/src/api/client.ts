import { http } from "./http";
import type {
    AuthResponse,
    LoginRequest,
    RegisterRequest,
    MeResponse,
    CreateShortUrlRequest,
    ShortUrlDto,
    PagedResult,
    ResolveResponse,
    UrlStatsResponse,
    UrlBreakdownResponse,
    ClickEventDto
} from "./contracts";

export const api = {
    auth: {
        register: (req: RegisterRequest) =>
            http.post<AuthResponse>("/api/auth/register", req),

        login: (req: LoginRequest) =>
            http.post<AuthResponse>("/api/auth/login", req),

        me: () =>
            http.get<MeResponse>("/api/auth/me"),

        logout: () =>
            http.post<void>("/api/auth/logout"),

        forgotPassword: (email: string) =>
            http.post<void>("/api/auth/forgot-password", { email }),

        resetPassword: (email: string, token: string, newPassword: string) =>
            http.post<void>("/api/auth/reset-password", {
                email,
                token,
                newPassword,
            }),
    },

    urls: {
        create: (req: CreateShortUrlRequest) =>
            http.post<ShortUrlDto>("/api/urls", req),

        list: (page = 1, pageSize = 20) =>
            http.get<PagedResult<ShortUrlDto>>(
                `/api/urls?page=${page}&pageSize=${pageSize}`
            ),

        get: (id: string) =>
            http.get<ShortUrlDto>(`/api/urls/${id}`),

        deactivate: (id: string) =>
            http.post<void>(`/api/urls/${id}/deactivate`),

        remove: (id: string) =>
            http.del<void>(`/api/urls/${id}`),

        stats: (id: string, from?: string, to?: string) => {
            const qs = new URLSearchParams();
            if (from) qs.set("from", from);
            if (to) qs.set("to", to);
            const tail = qs.toString() ? `?${qs.toString()}` : "";
            return http.get<UrlStatsResponse>(`/api/urls/${id}/stats${tail}`);
        },

        breakdown: (id: string, from?: string, to?: string) => {
            const qs = new URLSearchParams();
            if (from) qs.set("from", from);
            if (to) qs.set("to", to);
            const tail = qs.toString() ? `?${qs.toString()}` : "";
            return http.get<UrlBreakdownResponse>(`/api/urls/${id}/breakdown${tail}`);
        },

        clicks: (id: string, take = 50) =>
            http.get<ClickEventDto[]>(`/api/urls/${id}/clicks?take=${take}`),
    },

    public: {
        resolve: (shortCode: string) =>
            http.get<ResolveResponse>(`/api/public/resolve/${shortCode}`),
    },
};
