import { Suspense, useEffect, useMemo, useState, lazy } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../api/client";
import type { PagedResult, ShortUrlDto } from "../api/contracts";
import { ApiError } from "../api/http";
import { Button } from "@/components/ui/button";
import { CreateUrlCard } from "../features/urls/CreateUrlCard";
import { UrlsTable } from "../features/urls/UrlsTable";
import { toast } from "@/hooks/use-toast";
import { authStore } from "../auth/authStore";

// ✅ Lazy load analytics panel so dashboard initial load stays fast
const UrlAnalyticsModal = lazy(() =>
    import("../features/analytics/UrlAnalyticsModal").then((m) => ({
        default: m.UrlAnalyticsModal,
    }))
);

export default function DashboardPage() {
    const nav = useNavigate();

    const [data, setData] = useState<PagedResult<ShortUrlDto> | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    const [page, setPage] = useState(1);
    const pageSize = 10;

    // ✅ Selected for analytics modal
    const [selected, setSelected] = useState<{ id: string; shortCode: string } | null>(null);

    const baseRedirectUrl = useMemo(() => {
        const apiBase = (import.meta.env.VITE_API_BASE_URL as string) || "";
        return apiBase || window.location.origin.replace(":5173", ":5000");
    }, []);

    const handleApiError = (err: unknown, fallbackMsg = "Unexpected error") => {
        if (err instanceof ApiError) {
            const code = (err as any).status ?? (err as any).problem?.status ?? 0;

            if (code === 401 || code === 403) {
                authStore.clear();
                nav("/login", { replace: true });
                return true;
            }

            if (code >= 500 || code === 0) {
                nav("/error", {
                    state: {
                        code,
                        title: "Server error",
                        message: err.problem?.title ?? err.message ?? "Please try again later.",
                    },
                    replace: true,
                });
                return true;
            }

            setError(err.problem?.title ?? err.message);
            return false;
        }

        setError(fallbackMsg);
        return false;
    };

    const load = async () => {
        setLoading(true);
        setError("");

        try {
            const res = await api.urls.list(page, pageSize);
            setData(res);
        } catch (err) {
            handleApiError(err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        void load();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [page]);

    const onCreated = (item: ShortUrlDto) => {
        setPage(1);
        void load();

        toast({
            title: "Link created",
            description: `/${item.shortCode} is ready to share.`,
        });
    };

    const onDeactivate = async (id: string) => {
        try {
            await api.urls.deactivate(id);
            await load();
        } catch (err) {
            const redirected = handleApiError(err);
            if (!redirected) {
                toast({
                    title: "Deactivate failed",
                    description: "Please try again.",
                    variant: "destructive",
                });
            }
        }
    };

    const onDelete = async (id: string) => {
        try {
            await api.urls.remove(id);
            await load();
        } catch (err) {
            const redirected = handleApiError(err);
            if (!redirected) {
                toast({
                    title: "Delete failed",
                    description: "Please try again.",
                    variant: "destructive",
                });
            }
        }
    };

    // ✅ Toggle analytics on same row
    const onStats = (id: string, shortCode: string) => {
        setSelected((prev) => (prev?.id === id ? null : { id, shortCode }));
    };

    const total = data?.total ?? 0;
    const maxPage = Math.max(1, Math.ceil(total / pageSize));

    return (
        <div className="grid gap-6">
            <CreateUrlCard onCreated={onCreated} />

            {error && (
                <div className="rounded-2xl bg-red-500/10 ring-1 ring-red-500/30 px-4 py-3 text-sm text-red-200">
                    {error}
                </div>
            )}

            {loading ? (
                <div className="glass-surface p-6 text-white/70">Loading...</div>
            ) : (
                <>
                    <UrlsTable
                        items={data?.items ?? []}
                        baseRedirectUrl={baseRedirectUrl}
                        onDeactivate={onDeactivate}
                        onDelete={onDelete}
                        onStats={onStats}
                    />

                    <div className="flex items-center justify-between">
                        <div className="text-sm text-white/60">
                            Page {page} of {maxPage} · Total {total}
                        </div>
                        <div className="flex gap-2">
                            <Button variant="ghost" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
                                Prev
                            </Button>
                            <Button variant="ghost" disabled={page >= maxPage} onClick={() => setPage((p) => p + 1)}>
                                Next
                            </Button>
                        </div>
                    </div>
                </>
            )}

            {/* Glass modal overlay (doesn't push page down) */}
            {selected && (
                <Suspense
                    fallback={
                        <div className="fixed inset-0 z-50 grid place-items-center bg-black/55 backdrop-blur-sm">
                            <div className="rounded-3xl border border-white/10 bg-white/5 backdrop-blur-xl p-6 text-white/70 shadow-[0_30px_90px_rgba(0,0,0,0.45)]">
                                Loading analytics…
                            </div>
                        </div>
                    }
                >
                    <UrlAnalyticsModal
                        urlId={selected.id}
                        shortCode={selected.shortCode}
                        onClose={() => setSelected(null)}
                    />
                </Suspense>
            )}
        </div>
    );
}
