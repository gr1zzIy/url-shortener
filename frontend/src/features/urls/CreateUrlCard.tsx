import { useState } from "react";
import { api } from "@/api/client";
import { ApiError } from "@/api/http";
import type { ShortUrlDto } from "@/api/contracts";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";

export function CreateUrlCard({ onCreated }: { onCreated: (item: ShortUrlDto) => void }) {
    const [originalUrl, setOriginalUrl] = useState("");
    const [customCode, setCustomCode] = useState("");
    const [expiresAt, setExpiresAt] = useState("");
    const [error, setError] = useState("");
    const [busy, setBusy] = useState(false);

    const submit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError("");
        setBusy(true);

        try {
            const res = await api.urls.create({
                originalUrl,
                customCode: customCode.trim() ? customCode.trim() : null,
                expiresAt: expiresAt ? new Date(expiresAt).toISOString() : null,
            });

            onCreated(res);
            setOriginalUrl("");
            setCustomCode("");
            setExpiresAt("");
        } catch (err) {
            if (err instanceof ApiError) {
                const msg =
                    err.problem?.title ??
                    (err.problem?.errors ? Object.values(err.problem.errors).flat().join(", ") : err.message);
                setError(msg);
            } else setError("Unexpected error");
        } finally {
            setBusy(false);
        }
    };

    return (
        <Card className="fade-up text-white">
            <CardHeader>
                <CardTitle className="text-white">Create short link</CardTitle>
                <CardDescription className="text-white/60">
                    Paste a URL and get a short, shareable link.
                </CardDescription>
            </CardHeader>

            <CardContent>
                <form onSubmit={submit} className="grid gap-4">
                    <div className="grid gap-2">
                        <label className="text-sm text-white/70">Original URL</label>
                        <Input
                            value={originalUrl}
                            onChange={(e) => setOriginalUrl(e.target.value)}
                            placeholder="https://example.com/very/long/link"
                            className="bg-black/25 border-white/10 text-white placeholder:text-white/40"
                        />
                    </div>

                    <div className="grid gap-4 md:grid-cols-2">
                        <div className="grid gap-2">
                            <label className="text-sm text-white/70">Custom code (optional)</label>
                            <Input
                                value={customCode}
                                onChange={(e) => setCustomCode(e.target.value)}
                                placeholder="my-code"
                                className="bg-black/25 border-white/10 text-white placeholder:text-white/40"
                            />
                            <div className="text-xs text-white/50">Use letters or numbers.</div>
                        </div>

                        <div className="grid gap-2">
                            <label className="text-sm text-white/70">Expiration (optional)</label>
                            <Input
                                value={expiresAt}
                                onChange={(e) => setExpiresAt(e.target.value)}
                                type="datetime-local"
                                className="bg-black/25 border-white/10 text-white placeholder:text-white/40"
                            />
                            <div className="text-xs text-white/50">Leave empty to never expire.</div>
                        </div>
                    </div>

                    {error && (
                        <div className="rounded-xl bg-red-500/10 ring-1 ring-red-500/30 px-4 py-3 text-sm text-red-200">
                            {error}
                        </div>
                    )}

                    <div className="flex justify-end">
                        <Button
                            disabled={busy || !originalUrl.trim()}
                            className="bg-white text-black hover:bg-white/90"
                        >
                            {busy ? "Creating..." : "Create"}
                        </Button>
                    </div>
                </form>
            </CardContent>
        </Card>
    );
}
