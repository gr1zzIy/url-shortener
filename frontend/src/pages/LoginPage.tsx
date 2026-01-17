import { useEffect, useMemo, useRef, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { motion } from "framer-motion";
import { api } from "../api/client";
import { authStore } from "../auth/authStore";
import { ApiError } from "../api/http";
import { AuthCard } from "../ui/AuthCard";
import { Button } from "@/components/ui/button";
import { MotionField } from "../ui/MotionField";

function AsideLogin() {
    return (
        <div className="grid gap-3">
            <div className="rounded-2xl bg-black/20 ring-1 ring-white/10 p-4">
                <div className="text-sm font-medium text-white">What you get</div>
                <div className="mt-1 text-xs text-white/60 leading-relaxed">
                    Control, analytics, and a clean dashboard UX.
                </div>
            </div>

            <div className="rounded-2xl bg-black/20 ring-1 ring-white/10 p-4">
                <div className="text-sm font-medium text-white">Link management</div>
                <div className="mt-1 text-xs text-white/60 leading-relaxed">
                    Deactivate or delete links with confirmation dialogs.
                </div>
            </div>

            <div className="rounded-2xl bg-black/20 ring-1 ring-white/10 p-4">
                <div className="text-sm font-medium text-white">Analytics</div>
                <div className="mt-1 text-xs text-white/60 leading-relaxed">
                    Track clicks, unique visitors, and performance trends.
                </div>
            </div>

            <div className="text-xs text-white/40">
                Tip: press <span className="text-white/70">Tab</span> to jump between fields
            </div>
        </div>
    );
}

export default function LoginPage() {
    const nav = useNavigate();

    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");

    const [error, setError] = useState<string>("");
    const [isLoading, setIsLoading] = useState(false);

    const [showPassword, setShowPassword] = useState(false);
    const [capsLock, setCapsLock] = useState(false);

    const emailOk = useMemo(() => /^\S+@\S+\.\S+$/.test(email), [email]);
    const canSubmit = email.length > 0 && password.length > 0 && !isLoading;

    const passwordInputRef = useRef<HTMLInputElement | null>(null);

    useEffect(() => {
        const el = passwordInputRef.current;
        if (!el) return;

        const onKey = (e: KeyboardEvent) => {
            const caps = e.getModifierState?.("CapsLock") ?? false;
            setCapsLock(caps);
        };

        el.addEventListener("keydown", onKey);
        return () => el.removeEventListener("keydown", onKey);
    }, []);

    const onSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError("");

        try {
            setIsLoading(true);
            const r = await api.auth.login({ email, password });
            authStore.setAccessToken(r.accessToken);
            nav("/dashboard", { replace: true });
        } catch (err) {
            if (err instanceof ApiError) setError(err.problem?.title ?? err.message);
            else setError("Unexpected error");
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <AuthCard title="Welcome back" subtitle="Sign in to manage your short links." aside={<AsideLogin />}>
            <form onSubmit={onSubmit} className="grid gap-4 min-w-0">
                <div className="flex items-center justify-between">
          <span className="text-xs text-white/60">
            {email.length === 0 ? "" : emailOk ? "Looks good" : "Use a valid email"}
          </span>

                    <Link
                        to="/forgot-password"
                        className="text-xs text-white/60 hover:text-white underline underline-offset-4"
                    >
                        Forgot password?
                    </Link>
                </div>

                <MotionField
                    label="Email"
                    value={email}
                    onChange={setEmail}
                    placeholder="user@example.com"
                    inputMode="email"
                    autoComplete="email"
                />

                <div className="grid gap-2">
                    <MotionField
                        label="Password"
                        value={password}
                        onChange={setPassword}
                        type={showPassword ? "text" : "password"}
                        placeholder="••••••••"
                        autoComplete="current-password"
                        rightAdornment={
                            <button
                                type="button"
                                onClick={() => setShowPassword((v) => !v)}
                                className="text-xs text-white/70 hover:text-white transition-colors"
                            >
                                {showPassword ? "Hide" : "Show"}
                            </button>
                        }
                    />

                    <div
                        className={[
                            "text-xs transition-all",
                            capsLock ? "text-amber-200/90 opacity-100" : "text-white/40 opacity-0",
                        ].join(" ")}
                    >
                        Caps Lock is on
                    </div>
                </div>

                {error && (
                    <motion.div
                        key={error}
                        className="rounded-2xl bg-red-500/10 ring-1 ring-red-500/30 px-4 py-3 text-sm text-red-200"
                        initial={{ opacity: 0, y: -6 }}
                        animate={{ opacity: 1, y: 0, x: [0, -6, 6, -4, 4, -2, 2, 0] }}
                        transition={{ duration: 0.35 }}
                    >
                        {error}
                    </motion.div>
                )}

                <Button
                    className={["h-11 rounded-2xl bg-white text-black hover:bg-white/90", "transition-all"].join(" ")}
                    disabled={!canSubmit}
                >
                    {isLoading ? "Signing in..." : "Sign in"}
                </Button>

                <div className="flex items-center justify-between text-sm text-white/70">
                    <div>
                        No account?{" "}
                        <Link className="text-white underline underline-offset-4" to="/register">
                            Register
                        </Link>
                    </div>

                    <span className="text-xs text-white/40">
            Tip: press <span className="text-white/70">Tab</span>
          </span>
                </div>
            </form>
        </AuthCard>
    );
}