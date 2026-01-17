export type ProblemDetails = {
    type?: string;
    title?: string;
    status?: number;
    detail?: string;
    instance?: string;
    code?: string;
    traceId?: string;
    errors?: Record<string, string[]>;
};

export type AuthResponse = {
    accessToken: string;
    tokenType: string; // "Bearer"
    expiresIn: number; // seconds
};

export type RegisterRequest = {
    email: string;
    password: string;
};

export type LoginRequest = {
    email: string;
    password: string;
};

export type MeResponse = {
    id: string;
    email: string;
};

export type CreateShortUrlRequest = {
    originalUrl: string;
    customCode?: string | null;
    expiresAt?: string | null; // ISO string
};

export type ShortUrlDto = {
    id: string;
    shortCode: string;
    originalUrl: string;
    createdAt: string;
    expiresAt?: string | null;
    isActive: boolean;
    clicks: number;
    lastAccessedAt?: string | null;
};

export type PagedResult<T> = {
    items: T[];
    page: number;
    pageSize: number;
    total: number;
};

export type ResolveResponse = {
    shortCode: string;
    originalUrl: string;
    expiresAt?: string | null;
};

export type ClickSeriesPoint = {
    date: string;         // "2026-01-17"
    clicks: number;
    uniqueClicks: number;
};

export type UrlStatsResponse = {
    urlId: string;
    totalClicks: number;
    uniqueVisitors: number;
    series: ClickSeriesPoint[];
};

export type BreakdownItem = {
    key: string;   // "UA", "mobile", "Chrome"
    count: number;
};

export type UrlBreakdownResponse = {
    urlId: string;
    countries: BreakdownItem[];
    devices: BreakdownItem[];
    browsers: BreakdownItem[];
    os: BreakdownItem[];
};

export type ClickEventDto = {
    occurredAt: string;       // ISO
    ipAddress?: string | null;
    countryCode?: string | null;
    deviceType?: string | null;
    os?: string | null;
    browser?: string | null;
    userAgent?: string | null;
};
