function MiniCard({ title, body }: { title: string; body: string }) {
    return (
        <div className="rounded-2xl border border-white/10 bg-black/20 p-4">
            <div className="text-sm font-medium text-white">{title}</div>
            <div className="mt-1 text-xs text-white/60 leading-relaxed">{body}</div>
        </div>
    );
}

export function AuthAside({
                              mode,
                          }: {
    mode: "login" | "register" | "forgot";
}) {
    const cards =
        mode === "register"
            ? [
                { title: "Create links fast", body: "Shorten long URLs and share instantly." },
                { title: "Analytics", body: "Track clicks, unique visitors, device and country." },
                { title: "Control", body: "Deactivate links and manage lifecycle (expiry)." },
            ]
            : mode === "forgot"
                ? [
                    { title: "Privacy", body: "We’ll send a recovery link only if the email exists." },
                    { title: "Security", body: "Tokens expire and can be used once." },
                    { title: "Support", body: "If you didn’t request it, ignore the email." },
                ]
                : [
                    { title: "Dashboard", body: "Manage links in one place with a clean table UX." },
                    { title: "Insights", body: "View trends and breakdowns in a single modal." },
                    { title: "Safe patterns", body: "Consistent validation and error handling." },
                ];

    return (
        <div className="grid gap-3">
            {cards.map((c) => (
                <MiniCard key={c.title} title={c.title} body={c.body} />
            ))}

            <div className="pt-2 text-xs text-white/40">
                Tip: press <span className="text-white/65">Tab</span> to jump between fields
            </div>
        </div>
    );
}
