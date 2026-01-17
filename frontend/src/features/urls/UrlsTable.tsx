import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";
import { copyToClipboard } from "@/utils/copy";
import { formatDate, isExpired } from "@/utils/format";
import type { ShortUrlDto } from "@/api/contracts";
import { useMemo, useState } from "react";
import { middleEllipsis } from "@/utils/text";
import { ExternalLink, ArrowUpDown, Activity } from "lucide-react";
import { toast } from "@/hooks/use-toast";

type SortKey = "createdAt" | "clicks" | "lastAccessedAt";
type SortDir = "asc" | "desc";

function daysUntil(iso?: string | null) {
    if (!iso) return Number.POSITIVE_INFINITY;
    const ms = new Date(iso).getTime() - Date.now();
    return Math.ceil(ms / (1000 * 60 * 60 * 24));
}

export function UrlsTable({
                              items,
                              baseRedirectUrl,
                              onDeactivate,
                              onDelete,
                              onStats,
                          }: {
    items: ShortUrlDto[];
    baseRedirectUrl: string;
    onDeactivate: (id: string) => Promise<void>;
    onDelete: (id: string) => Promise<void>;
    onStats?: (id: string, shortCode: string) => void;
}) {
    const [copied, setCopied] = useState<string | null>(null);
    const [sortKey, setSortKey] = useState<SortKey>("createdAt");
    const [sortDir, setSortDir] = useState<SortDir>("desc");
    const [query, setQuery] = useState("");

    const [confirm, setConfirm] = useState<
        | { type: "deactivate"; item: ShortUrlDto }
        | { type: "delete"; item: ShortUrlDto }
        | null
    >(null);
    const [actionBusy, setActionBusy] = useState(false);

    const filtered = useMemo(() => {
        const q = query.trim().toLowerCase();
        if (!q) return items;
        return items.filter((x) => {
            return x.shortCode.toLowerCase().includes(q) || x.originalUrl.toLowerCase().includes(q);
        });
    }, [items, query]);

    const sorted = useMemo(() => {
        const dir = sortDir === "asc" ? 1 : -1;
        const getTime = (v?: string | null) => (v ? new Date(v).getTime() : 0);

        return [...filtered].sort((a, b) => {
            if (sortKey === "clicks") return (a.clicks - b.clicks) * dir;
            if (sortKey === "lastAccessedAt") return (getTime(a.lastAccessedAt) - getTime(b.lastAccessedAt)) * dir;
            return (getTime(a.createdAt) - getTime(b.createdAt)) * dir;
        });
    }, [filtered, sortDir, sortKey]);

    const toggleSort = (key: SortKey) => {
        if (sortKey === key) setSortDir(sortDir === "asc" ? "desc" : "asc");
        else {
            setSortKey(key);
            setSortDir("desc");
        }
    };

    const copy = async (text: string, id: string) => {
        await copyToClipboard(text);
        setCopied(id);
        window.setTimeout(() => setCopied(null), 1200);

        toast({
            title: "Copied",
            description: "Short link copied to clipboard.",
        });
    };

    const runConfirmed = async () => {
        if (!confirm) return;
        setActionBusy(true);

        try {
            if (confirm.type === "deactivate") {
                await onDeactivate(confirm.item.id);
                toast({
                    title: "Link deactivated",
                    description: `/${confirm.item.shortCode} is now inactive.`,
                });
            } else {
                await onDelete(confirm.item.id);
                toast({
                    title: "Link deleted",
                    description: `/${confirm.item.shortCode} has been removed.`,
                });
            }
            setConfirm(null);
        } catch {
            toast({
                title: "Action failed",
                description: "Please try again.",
                variant: "destructive",
            });
        } finally {
            setActionBusy(false);
        }
    };

    return (
        <>
            <div className="glass-surface fade-up">
                {/* Header */}
                <div className="flex flex-col gap-3 px-5 py-4 sm:flex-row sm:items-center sm:justify-between">
                    <div>
                        <div className="text-sm font-semibold text-white">Your links</div>
                        <div className="text-xs text-white/60">Create, manage, and track performance.</div>
                    </div>

                    <div className="w-full sm:w-[340px]">
                        <Input
                            value={query}
                            onChange={(e) => setQuery(e.target.value)}
                            placeholder="Search by short code or original URL..."
                            className="bg-black/25 border-white/10 text-white placeholder:text-white/40"
                        />
                    </div>
                </div>

                <div className="px-2 pb-2">
                    <Table className="table-fixed">
                        <TableHeader>
                            <TableRow>
                                <TableHead className="w-[180px]">Short</TableHead>

                                <TableHead>
                                    <button
                                        type="button"
                                        onClick={() => toggleSort("createdAt")}
                                        className="inline-flex items-center gap-1 text-white/70 hover:text-white transition-colors"
                                    >
                                        Original <ArrowUpDown className="h-3.5 w-3.5" />
                                    </button>
                                </TableHead>

                                <TableHead className="w-[220px]">
                                    <button
                                        type="button"
                                        onClick={() => toggleSort("clicks")}
                                        className="inline-flex items-center gap-1 text-white/70 hover:text-white transition-colors"
                                    >
                                        Stats <ArrowUpDown className="h-3.5 w-3.5" />
                                    </button>
                                </TableHead>

                                <TableHead className="w-[140px]">Status</TableHead>

                                <TableHead className="w-[260px] text-right">Actions</TableHead>
                            </TableRow>
                        </TableHeader>

                        <TableBody>
                            {sorted.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={5} className="py-10 text-center text-sm text-white/60">
                                        {items.length === 0
                                            ? "No links yet. Create your first one above."
                                            : "No results. Try a different search."}
                                    </TableCell>
                                </TableRow>
                            ) : (
                                sorted.map((x) => {
                                    const shortUrl = `${baseRedirectUrl}/${x.shortCode}`;
                                    const expired = isExpired(x.expiresAt);
                                    const dLeft = daysUntil(x.expiresAt);
                                    const expiringSoon = x.isActive && !expired && dLeft <= 3;

                                    const status =
                                        !x.isActive ? "Inactive" : expired ? "Expired" : expiringSoon ? "Expiring" : "Active";

                                    const rowCellHover = "transition-colors duration-200 group-hover:bg-white/5";
                                    const roundedEnds = "first:rounded-l-2xl last:rounded-r-2xl";

                                    return (
                                        <TableRow key={x.id} className="group align-top">
                                            <TableCell className={`py-4 ${rowCellHover} ${roundedEnds}`}>
                                                <div className="font-medium text-white">{x.shortCode}</div>

                                                <div className="mt-1 flex items-center gap-3 text-xs">
                                                    <a
                                                        href={shortUrl}
                                                        target="_blank"
                                                        rel="noreferrer"
                                                        className="inline-flex items-center gap-1 text-white/70 hover:text-white underline underline-offset-4 transition-colors"
                                                        title="Open short link"
                                                    >
                                                        <ExternalLink className="h-3.5 w-3.5" />
                                                        Open
                                                    </a>

                                                    <button
                                                        type="button"
                                                        onClick={() => copy(shortUrl, x.id)}
                                                        className="text-white/70 underline underline-offset-4 hover:text-white transition-colors"
                                                    >
                                                        {copied === x.id ? "Copied" : "Copy"}
                                                    </button>
                                                </div>
                                            </TableCell>

                                            <TableCell className={`py-4 ${rowCellHover}`}>
                                                <a
                                                    href={x.originalUrl}
                                                    target="_blank"
                                                    rel="noreferrer"
                                                    title={x.originalUrl}
                                                    className="block max-w-[720px] text-sm text-white/80 hover:underline underline-offset-4 break-words transition-colors"
                                                >
                                                    {middleEllipsis(x.originalUrl, 40, 15)}
                                                </a>

                                                <div className="mt-2 flex flex-col gap-0.5 text-xs text-white/60">
                                                    <div>Created: {formatDate(x.createdAt)}</div>
                                                    <div className={expiringSoon ? "text-amber-200/90" : undefined}>
                                                        Expires: {formatDate(x.expiresAt)}
                                                        {expiringSoon ? ` (in ${dLeft} day${dLeft === 1 ? "" : "s"})` : ""}
                                                    </div>
                                                </div>
                                            </TableCell>

                                            <TableCell className={`py-4 ${rowCellHover}`}>
                                                <div className="flex flex-col gap-1">
                                                    <Badge variant="secondary" className="bg-white/10 text-white/80 w-fit">
                                                        {x.clicks} clicks
                                                    </Badge>
                                                    <Badge variant="secondary" className="bg-white/10 text-white/80 w-fit">
                                                        Last: {formatDate(x.lastAccessedAt)}
                                                    </Badge>
                                                </div>
                                            </TableCell>

                                            <TableCell className={`py-4 ${rowCellHover}`}>
                                                <Badge
                                                    variant="secondary"
                                                    className={
                                                        status === "Active"
                                                            ? "bg-emerald-500/15 text-emerald-100 border-emerald-500/30"
                                                            : status === "Expiring"
                                                                ? "bg-amber-500/15 text-amber-100 border-amber-500/30"
                                                                : status === "Expired"
                                                                    ? "bg-orange-500/15 text-orange-100 border-orange-500/30"
                                                                    : "bg-white/10 text-white/70 border-white/10"
                                                    }
                                                >
                                                    {status}
                                                </Badge>
                                            </TableCell>

                                            <TableCell className={`py-4 text-right ${rowCellHover} ${roundedEnds}`}>
                                                <div className="flex justify-end gap-2 flex-wrap">
                                                    {/* ✅ Stats */}
                                                    {onStats && (
                                                        <Button
                                                            variant="secondary"
                                                            className="bg-white/10 text-white hover:bg-white/15 transition-colors rounded-2xl"
                                                            onClick={() => onStats(x.id, x.shortCode)}
                                                            title="Open analytics"
                                                        >
                                                            <Activity className="h-4 w-4 mr-1.5" />
                                                            Stats
                                                        </Button>
                                                    )}

                                                    <Button
                                                        variant="secondary"
                                                        className="bg-white/10 text-white hover:bg-white/15 transition-colors rounded-2xl"
                                                        disabled={!x.isActive}
                                                        onClick={() => setConfirm({ type: "deactivate", item: x })}
                                                    >
                                                        {x.isActive ? "Deactivate" : "Inactive"}
                                                    </Button>

                                                    <Button
                                                        variant="destructive"
                                                        className="rounded-2xl transition-colors"
                                                        onClick={() => setConfirm({ type: "delete", item: x })}
                                                    >
                                                        Delete
                                                    </Button>
                                                </div>
                                            </TableCell>
                                        </TableRow>
                                    );
                                })
                            )}
                        </TableBody>
                    </Table>
                </div>
            </div>

            <Dialog open={!!confirm} onOpenChange={(open) => (!open ? setConfirm(null) : null)}>
                <DialogContent>
                    <DialogHeader>
                        <DialogTitle>
                            {confirm?.type === "delete" ? "Delete this link?" : "Deactivate this link?"}
                        </DialogTitle>
                        <DialogDescription>
                            {confirm?.type === "delete"
                                ? "This will permanently remove the short link and its stats."
                                : "The short link will stop redirecting, but you can keep it for reference."}
                        </DialogDescription>
                    </DialogHeader>

                    {confirm && (
                        <div className="rounded-2xl border border-white/10 bg-black/20 p-4">
                            <div className="text-sm text-white">
                                <span className="text-white/70">Short:</span> /{confirm.item.shortCode}
                            </div>
                            <div className="mt-1 break-words text-sm text-white/70">{confirm.item.originalUrl}</div>
                        </div>
                    )}

                    <DialogFooter>
                        <Button
                            variant="secondary"
                            className="bg-white/10 text-white hover:bg-white/15 rounded-2xl transition-colors"
                            disabled={actionBusy}
                            onClick={() => setConfirm(null)}
                        >
                            Cancel
                        </Button>
                        <Button
                            variant={confirm?.type === "delete" ? "destructive" : "default"}
                            className={[
                                confirm?.type === "delete" ? "" : "bg-white text-black hover:bg-white/90",
                                "rounded-2xl transition-colors",
                            ].join(" ")}
                            disabled={actionBusy}
                            onClick={runConfirmed}
                        >
                            {actionBusy ? "Working..." : confirm?.type === "delete" ? "Delete" : "Deactivate"}
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </>
    );
}
