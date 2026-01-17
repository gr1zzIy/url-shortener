import { Outlet, useNavigate } from "react-router-dom";
import { useState } from "react";
import { api } from "../api/client";
import { authStore } from "../auth/authStore";
import { Logo } from "../ui/Logo";
import { GlassThemeSwitch } from "../ui/GlassThemeSwitch";

export default function AppShell() {
    const nav = useNavigate();
    const [loggingOut, setLoggingOut] = useState(false);

    const logout = async () => {
        if (loggingOut) return;

        setLoggingOut(true);
        try {
            await api.auth.logout();
        } finally {
            authStore.clear();
            nav("/login", { replace: true });
            setLoggingOut(false);
        }
    };

    return (
        <div className="app-bg">
            <div className="min-h-screen text-white">
                <div className="mx-auto max-w-6xl px-4 py-6">
                    <header className="glass-surface fade-up px-5 py-4">
                        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                            {/* LEFT */}
                            <div className="flex items-center gap-3 min-w-0">
                                <Logo />
                                <span className="hidden sm:inline-flex glass-chip shrink-0">Dashboard</span>
                            </div>

                            {/* RIGHT */}
                            <div className="flex items-center justify-between gap-3 sm:justify-end">
                                <div className="min-w-0">
                                    <GlassThemeSwitch />
                                </div>

                                <button
                                    onClick={logout}
                                    className={[
                                        "glass-button",
                                        "whitespace-nowrap",
                                        loggingOut ? "opacity-70 pointer-events-none" : "",
                                    ].join(" ")}
                                >
                                    {loggingOut ? "Logging out..." : "Logout"}
                                </button>
                            </div>
                        </div>
                    </header>

                    <main className="mt-6">
                        <Outlet />
                    </main>
                </div>
            </div>
        </div>
    );
}
