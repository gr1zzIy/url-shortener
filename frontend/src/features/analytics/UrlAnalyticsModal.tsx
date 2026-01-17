import { useEffect, useMemo, useState } from "react";
import { motion } from "framer-motion";
import { api } from "@/api/client.ts";
import type { ClickEventDto, UrlBreakdownResponse, UrlStatsResponse } from "@/api/contracts.ts";
import { ApiError } from "@/api/http.ts";
import { Button } from "@/components/ui/button";
import { X } from "lucide-react";
import {
    ResponsiveContainer,
    LineChart,
    Line,
    XAxis,
    YAxis,
    Tooltip,
    CartesianGrid,
} from "recharts";

function formatDateISO(d: Date) {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, "0");
    const day = String(d.getDate()).padStart(2, "0");
    return `${y}-${m}-${day}`;
}
function lastNDaysRange(n: number) {
    const to = new Date();
    const from = new Date();
    from.setDate(to.getDate() - (n - 1));
    return { from: formatDateISO(from), to: formatDateISO(to) };
}

function StatCard({ label, value, hint }: { label: string; value: string; hint?: string }) {
    return (
        <div className="rounded-3xl border border-white/10 bg-white/5 backdrop-blur-xl p-5 shadow-[0_20px_60px_rgba(0,0,0,0.35)]">
            <div className="text-xs text-white/60">{label}</div>
            <div className="mt-2 text-2xl font-semibold text-white tracking-tight">{value}</div>
            {hint && <div className="mt-1 text-xs text-white/40">{hint}</div>}
        </div>
    );
}

function Pill({ text }: { text: string }) {
    return (
        <span className="rounded-full bg-white/10 ring-1 ring-white/10 px-2.5 py-1 text-[11px] text-white/70">
      {text}
    </span>
    );
}

function TopList({
                     title,
                     items,
                     emptyText,
                 }: {
    title: string;
    items?: { key: string; count: number }[] | null;
    emptyText: string;
}) {
    const safe = items ?? [];

    return (
        <div className="rounded-3xl border border-white/10 bg-white/5 backdrop-blur-xl p-5 shadow-[0_20px_60px_rgba(0,0,0,0.35)]">
            <div className="flex items-center justify-between">
                <div className="text-sm font-semibold text-white">{title}</div>
                <div className="text-xs text-white/40">top</div>
            </div>

            <div className="mt-4 flex flex-wrap gap-2">
                {safe.length === 0 ? (
                    <span className="text-xs text-white/40">{emptyText}</span>
                ) : (
                    safe.slice(0, 10).map((x) => <Pill key={x.key} text={`${x.key} · ${x.count}`} />)
                )}
            </div>
        </div>
    );
}

function RecentClicks({ items }: { items: ClickEventDto[] }) {
    return (
        <div className="rounded-3xl border border-white/10 bg-white/5 backdrop-blur-xl p-5 shadow-[0_20px_60px_rgba(0,0,0,0.35)]">
            <div className="flex items-center justify-between">
                <div className="text-sm font-semibold text-white">Recent clicks</div>
                <div className="text-xs text-white/40">{items.length}</div>
            </div>

            <div className="mt-4 overflow-hidden rounded-2xl ring-1 ring-white/10">
                <div className="grid grid-cols-12 gap-2 bg-black/20 px-3 py-2 text-[11px] text-white/60">
                    <div className="col-span-4">Time</div>
                    <div className="col-span-2">Country</div>
                    <div className="col-span-2">Device</div>
                    <div className="col-span-2">Browser</div>
                    <div className="col-span-2">IP</div>
                </div>

                <div className="max-h-[260px] overflow-auto">
                    {items.length === 0 ? (
                        <div className="px-3 py-4 text-xs text-white/40">No clicks yet.</div>
                    ) : (
                        items.map((x, idx) => (
                            <div
                                key={`${x.occurredAt}-${idx}`}
                                className="grid grid-cols-12 gap-2 px-3 py-2 text-[12px] text-white/75 border-t border-white/5"
                            >
                                <div className="col-span-4 text-white/70">{new Date(x.occurredAt).toLocaleString()}</div>
                                <div className="col-span-2">{x.countryCode ?? "—"}</div>
                                <div className="col-span-2">{x.deviceType ?? "—"}</div>
                                <div className="col-span-2">{x.browser ?? "—"}</div>
                                <div className="col-span-2">{x.ipAddress ?? "—"}</div>
                            </div>
                        ))
                    )}
                </div>
            </div>
        </div>
    );
}

export function UrlAnalyticsModal({
                                      urlId,
                                      shortCode,
                                      onClose,
                                  }: {
    urlId: string;
    shortCode: string;
    onClose: () => void;
}) {
    const range7 = useMemo(() => lastNDaysRange(7), []);
    const [from, setFrom] = useState(range7.from);
    const [to, setTo] = useState(range7.to);

    const [stats, setStats] = useState<UrlStatsResponse | null>(null);
    const [breakdown, setBreakdown] = useState<UrlBreakdownResponse | null>(null);
    const [clicks, setClicks] = useState<ClickEventDto[]>([]);

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const onKey = (e: KeyboardEvent) => {
            if (e.key === "Escape") onClose();
        };
        window.addEventListener("keydown", onKey);
        return () => window.removeEventListener("keydown", onKey);
    }, [onClose]);

    useEffect(() => {
        let cancelled = false;

        const load = async () => {
            setLoading(true);
            setError("");
            try {
                const [s, b, c] = await Promise.all([
                    api.urls.stats(urlId, from, to),
                    api.urls.breakdown(urlId, from, to),
                    api.urls.clicks(urlId, 50),
                ]);
                if (cancelled) return;
                setStats(s);
                setBreakdown(b);
                setClicks(c);
            } catch (err) {
                if (cancelled) return;
                if (err instanceof ApiError) setError(err.problem?.title ?? err.message);
                else setError("Unexpected error");
            } finally {
                if (!cancelled) setLoading(false);
            }
        };

        void load();
        return () => {
            cancelled = true;
        };
    }, [urlId, from, to]);
    const chartData = useMemo(() => {
        const series: any[] = (stats as any)?.series ?? [];
        return series.map((p) => ({
            date: p.day ?? p.date, // backend: day
            clicks: typeof p.clicks === "number" ? p.clicks : 0,
            uniqueClicks: typeof p.uniqueClicks === "number" ? p.uniqueClicks : 0,
        }));
    }, [stats]);

    return (
        <div className="fixed inset-0 z-50 overflow-x-hidden">
            {/* backdrop */}
            <div className="absolute inset-0 bg-black/55 backdrop-blur-sm" onClick={onClose} />

            {/* sheet */}
            <motion.div
                className="absolute inset-x-0 top-6 mx-auto w-[min(1100px,calc(100%-24px))] px-3"
                initial={{ opacity: 0, y: 14, scale: 0.985 }}
                animate={{ opacity: 1, y: 0, scale: 1 }}
                transition={{ duration: 0.22 }}
            >
                <div className="max-w-full rounded-[28px] border border-white/10 bg-white/5 backdrop-blur-xl shadow-[0_40px_120px_rgba(0,0,0,0.6)] overflow-hidden">
                    <div className="flex items-start justify-between gap-4 px-6 py-5 bg-black/20 border-b border-white/10">
                        <div>
                            <div className="text-xs text-white/60">Analytics</div>
                            <div className="mt-1 text-lg font-semibold text-white tracking-tight">/{shortCode}</div>
                        </div>

                        <div className="flex items-center gap-2">
                            <input
                                value={from}
                                onChange={(e) => setFrom(e.target.value)}
                                type="date"
                                className="h-10 rounded-2xl bg-black/20 border border-white/10 px-3 text-sm text-white/80 outline-none focus:ring-2 focus:ring-white/20"
                            />
                            <input
                                value={to}
                                onChange={(e) => setTo(e.target.value)}
                                type="date"
                                className="h-10 rounded-2xl bg-black/20 border border-white/10 px-3 text-sm text-white/80 outline-none focus:ring-2 focus:ring-white/20"
                            />

                            <Button variant="ghost" onClick={onClose} className="gap-2">
                                <X className="h-4 w-4" />
                                Close
                            </Button>
                        </div>
                    </div>

                    <div className="max-h-[calc(100vh-140px)] overflow-auto p-6">
                        {error && (
                            <div className="mb-5 rounded-2xl bg-red-500/10 ring-1 ring-red-500/30 px-4 py-3 text-sm text-red-200">
                                {error}
                            </div>
                        )}

                        {loading || !stats || !breakdown ? (
                            <div className="text-white/70">Loading analytics…</div>
                        ) : (
                            <div className="grid gap-5">
                                <div className="grid gap-4 md:grid-cols-3">
                                    <StatCard label="Total clicks" value={String(stats.totalClicks)} />
                                    <StatCard label="Unique visitors" value={String(stats.uniqueVisitors)} hint="visitor fingerprint" />
                                    <StatCard label="Period" value={`${from} → ${to}`} />
                                </div>

                                {/* chart */}
                                <div className="rounded-3xl border border-white/10 bg-white/5 backdrop-blur-xl p-5 shadow-[0_20px_60px_rgba(0,0,0,0.35)]">
                                    <div className="flex items-center justify-between">
                                        <div className="text-sm font-semibold text-white">Clicks over time</div>
                                        <div className="text-xs text-white/50">{chartData.length} days</div>
                                    </div>

                                    <div className="mt-4 h-[260px]">
                                        <ResponsiveContainer width="100%" height="100%">
                                            <LineChart data={chartData}>
                                                <CartesianGrid strokeOpacity={0.15} />
                                                <XAxis
                                                    dataKey="date"
                                                    tickFormatter={(v) => String(v).slice(5)} // MM-DD
                                                    tick={{ fill: "rgba(255,255,255,0.55)", fontSize: 11 }}
                                                    axisLine={{ stroke: "rgba(255,255,255,0.12)" }}
                                                    tickLine={{ stroke: "rgba(255,255,255,0.12)" }}
                                                />
                                                <YAxis
                                                    tick={{ fill: "rgba(255,255,255,0.55)", fontSize: 11 }}
                                                    axisLine={{ stroke: "rgba(255,255,255,0.12)" }}
                                                    tickLine={{ stroke: "rgba(255,255,255,0.12)" }}
                                                    allowDecimals={false}
                                                />
                                                <Tooltip
                                                    labelFormatter={(v) => `Date: ${v}`}
                                                    contentStyle={{
                                                        background: "rgba(10,10,10,0.65)",
                                                        border: "1px solid rgba(255,255,255,0.15)",
                                                        borderRadius: 16,
                                                        backdropFilter: "blur(16px)",
                                                        color: "rgba(255,255,255,0.85)",
                                                    }}
                                                    labelStyle={{ color: "rgba(255,255,255,0.75)" }}
                                                />
                                                <Line
                                                    type="monotone"
                                                    dataKey="clicks"
                                                    strokeWidth={2.5}
                                                    dot={false}
                                                    stroke="rgba(255,255,255,0.85)"
                                                />
                                                <Line
                                                    type="monotone"
                                                    dataKey="uniqueClicks"
                                                    strokeWidth={2}
                                                    dot={false}
                                                    stroke="rgba(255,255,255,0.45)"
                                                />
                                            </LineChart>
                                        </ResponsiveContainer>
                                    </div>
                                </div>

                                <div className="grid gap-4 md:grid-cols-2">
                                    <TopList title="Countries" items={breakdown.countries ?? []} emptyText="No country data yet." />
                                    <TopList title="Devices" items={breakdown.devices ?? []} emptyText="No device data yet." />
                                    <TopList title="Browsers" items={breakdown.browsers ?? []} emptyText="No browser data yet." />
                                    <TopList title="OS" items={breakdown.os ?? []} emptyText="No OS data yet." />
                                </div>

                                <RecentClicks items={clicks} />
                            </div>
                        )}
                    </div>
                </div>
            </motion.div>
        </div>
    );
}