/** @typedef {{ id: string, email: string }} MeResponse */

/**
 * @typedef {Object} ShortUrlDto
 * @property {string} id
 * @property {string} shortCode
 * @property {string} originalUrl
 * @property {number} clicks
 * @property {boolean} isActive
 */

/**
 * @typedef {Object} UrlStatsResponse
 * @property {number} totalClicks
 * @property {number} uniqueVisitors
 * @property {Array<{date: string, clicks: number}>} series
 */

/**
 * @typedef {Object} UrlBreakdownResponse
 * @property {Array<{key: string, count: number}>} countries
 * @property {Array<{key: string, count: number}>} devices
 * @property {Array<{key: string, count: number}>} browsers
 * @property {Array<{key: string, count: number}>} os
 */

/**
 * @typedef {Object} ClickEventDto
 * @property {string} occurredAt
 * @property {string=} countryCode
 * @property {string=} deviceType
 * @property {string=} browser
 * @property {string=} os
 */

/**
 * @typedef {Object} AppState
 * @property {ShortUrlDto[]} urls
 * @property {MeResponse | null} user
 * @property {string} activeAuthTab
 */

/**
 * @typedef {Object} SessionHandlers
 * @property {() => Promise<void>} bootstrap
 * @property {(view: string) => void} showOnly
 * @property {(tab: string) => void} setAuthTab
 */

/** @typedef {(path: string, options?: { method?: string, body?: unknown, withAuth?: boolean, retryOn401?: boolean }) => Promise<any>} ApiRequest */

export {};

