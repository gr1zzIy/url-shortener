import { useMemo, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { motion } from "framer-motion";
import { api } from "../api/client";
import { authStore } from "../auth/authStore";
import { ApiError } from "../api/http";
import { AuthCard } from "../ui/AuthCard";
import { Button } from "@/components/ui/button";
import { MotionField } from "../ui/MotionField";

function AsideRegister() {
    return (
        <div className="grid gap-3">
            <div className="rounded-2xl bg-black/20 ring-1 ring-white/10 p-4">
                <div className="text-sm font-medium text-white">Quick start</div>
                <div className="mt-1 text-xs text-white/60 leading-relaxed">
                    Create your first link in under a minute.
                </div>
            </div>

            <ol className="grid gap-3 text-sm text-white/70 list-decimal list-inside">
                <li className="rounded-2xl bg-black/20 ring-1 ring-white/10 p-4">Create an account.</li>
                <li className="rounded-2xl bg-black/20 ring-1 ring-white/10 p-4">Paste a URL and generate a code.</li>
                <li className="rounded-2xl bg-black/20 ring-1 ring-white/10 p-4">Share and watch clicks.</li>
            </ol>

            <div className="text-xs text-white/40">
                You can deactivate or delete links anytime (with confirmation).
            </div>
        </div>
    );
}

export default function RegisterPage() {
    const nav = useNavigate();

    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");

    const [error, setError] = useState<string>("");
    const [isLoading, setIsLoading] = useState(false);

    const [showPassword, setShowPassword] = useState(false);

    const emailOk = useMemo(() => /^\S+@\S+\.\S+$/.test(email), [email]);

    const strength = useMemo(() => {
        const p = password;
        if (!p) return { label: "Start typing", score: 0 };
        let s = 0;
        if (p.length >= 8) s++;
        if (/[A-Z]/.test(p)) s++;
        if (/[0-9]/.test(p)) s++;
        if (/[^A-Za-z0-9]/.test(p)) s++;
        const label = s <= 1 ? "Weak" : s === 2 ? "Okay" : s === 3 ? "Good" : "Strong";
        return { label, score: s };
    }, [password]);

    const pct = Math.min(100, (strength.score / 4) * 100);
    const canSubmit = emailOk && password.length >= 8 && !isLoading;

    const onSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError("");

        try {
            setIsLoading(true);
            const r = await api.auth.register({ email, password });
            authStore.setAccessToken(r.accessToken);
            nav("/dashboard", { replace: true });
        } catch (err) {
            if (err instanceof ApiError) {
                const msg =
                    err.problem?.title ??
                    (err.problem?.errors ? Object.values(err.problem.errors).flat().join(", ") : err.message);
                setError(msg);
            } else setError("Unexpected error");
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <AuthCard
            title="Create your account"
            subtitle="Start shortening links with stats and control."
            aside={<AsideRegister />}
        >
            <form onSubmit={onSubmit} className="grid gap-4 min-w-0">
                <div className="flex items-center justify-between">
          <span className="text-xs text-white/60">
            {email.length === 0 ? "" : emailOk ? "Valid email" : "Enter a valid email"}
          </span>
                    <Link
                        to="/login"
                        className="text-xs text-white/60 hover:text-white underline underline-offset-4 transition-colors"
                    >
                        Already have an account?
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

                <MotionField
                    label="Password"
                    value={password}
                    onChange={setPassword}
                    type={showPassword ? "text" : "password"}
                    placeholder="Min 8 chars"
                    autoComplete="new-password"
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

                <div className="mt-1 grid gap-1">
                    <div className="flex items-center justify-between text-xs">
                        <span className="text-white/50">Password strength</span>
                        <span className="text-white/70">{strength.label}</span>
                    </div>

                    <div className="h-2 rounded-full bg-white/10 overflow-hidden">
                        <motion.div
                            className="h-full rounded-full bg-white/60"
                            initial={false}
                            animate={{ width: `${pct}%`, scaleY: password ? 1 : 0.75 }}
                            transition={{ type: "spring", stiffness: 320, damping: 28 }}
                            style={{ transformOrigin: "left center" }}
                        />
                    </div>

                    <div className="text-xs text-white/40">
                        Use 8+ chars, add a number and a symbol for a stronger password.
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

                <Button className="h-11 rounded-2xl bg-white text-black hover:bg-white/90" disabled={!canSubmit}>
                    {isLoading ? "Creating..." : "Create account"}
                </Button>

                <div className="text-sm text-white/70">
                    Have an account?{" "}
                    <Link className="text-white underline underline-offset-4" to="/login">
                        Login
                    </Link>
                </div>
            </form>
        </AuthCard>
    );
}