import { Link, useLocation } from "react-router-dom";
import { authStore } from "../auth/authStore";

export function Logo() {
    const brand = "GlassLink";
    const tagline = "short links, clean control";

    const isAuthed = !!authStore.getAccessToken();
    const to = isAuthed ? "/dashboard" : "/login";

    const loc = useLocation();
    const isSameRoute = loc.pathname === to;

    return (
        <Link
            to={to}
            aria-label={isAuthed ? "Go to dashboard" : "Go to login"}
            title={isAuthed ? "Dashboard" : "Login"}
            className={[
                "group flex items-center gap-3 select-none",
                "rounded-2xl focus:outline-none focus-visible:ring-2 focus-visible:ring-white/30",
                isSameRoute ? "cursor-default" : "cursor-pointer",
            ].join(" ")}
            onClick={(e) => {
                // Optional: prevent "re-navigate" to same route
                if (isSameRoute) e.preventDefault();
            }}
        >
            {/* Mark */}
            <div className="relative h-10 w-10">
                <div
                    className={[
                        "absolute inset-0 rounded-2xl bg-white/10 ring-1 ring-white/15 backdrop-blur-xl",
                        "shadow-[0_10px_30px_rgba(0,0,0,0.35)]",
                        "transition-transform duration-200",
                        !isSameRoute ? "group-hover:scale-[1.02] active:scale-[0.98]" : "",
                    ].join(" ")}
                />
                <div
                    className="absolute inset-0 rounded-2xl opacity-70"
                    style={{
                        background:
                            "radial-gradient(120% 120% at 30% 20%, rgba(255,255,255,0.35) 0%, rgba(255,255,255,0) 55%)",
                    }}
                />
                <div className="relative z-10 grid h-full w-full place-items-center">
                    <span className="text-xs font-semibold tracking-wide text-white">GL</span>
                </div>
            </div>

            {/* Text */}
            <div className="leading-tight">
                <div className="flex items-center gap-2">
                    <div className="text-sm font-semibold text-white tracking-tight">
                        {brand}
                    </div>
                    <span className="rounded-full bg-white/10 ring-1 ring-white/10 px-2 py-0.5 text-[10px] text-white/70">
                        beta
                    </span>
                </div>
                <div className="text-xs text-white/60">{tagline}</div>
            </div>
        </Link>
    );
}
