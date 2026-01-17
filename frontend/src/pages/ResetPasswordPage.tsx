import { useSearchParams, useNavigate, Link } from "react-router-dom";
import { useMemo, useState } from "react";
import { AuthCard } from "../ui/AuthCard";
import { MotionField } from "../ui/MotionField";
import { Button } from "@/components/ui/button";
import { api } from "../api/client";
import { motion } from "framer-motion";

export default function ResetPasswordPage() {
    const [params] = useSearchParams();
    const nav = useNavigate();

    const email = params.get("email") ?? "";
    const token = params.get("token") ?? "";

    const [password, setPassword] = useState("");
    const [loading, setLoading] = useState(false);
    const [done, setDone] = useState(false);

    const canSubmit = useMemo(() => {
        if (!email || !token) return false;
        return password.length >= 6 && !loading;
    }, [email, token, password, loading]);

    const submit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        try {
            await api.auth.resetPassword(email, token, password);
            setDone(true);
        } finally {
            setLoading(false);
        }
    };

    return (
        <AuthCard
            title="Set new password"
            subtitle="Choose a new secure password."
        >
            <div className="flex items-center justify-between">
                <button
                    type="button"
                    onClick={() => nav(-1)}
                    className="text-xs text-white/60 hover:text-white transition-colors"
                >
                    ← Back
                </button>

                <Link
                    to="/login"
                    className="text-xs text-white/60 hover:text-white underline underline-offset-4"
                >
                    Back to login
                </Link>
            </div>

            {!email || !token ? (
                <motion.div
                    className="mt-4 rounded-2xl bg-amber-500/10 ring-1 ring-amber-500/25 px-4 py-3 text-sm text-amber-100"
                    initial={{ opacity: 0, y: -6 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.25 }}
                >
                    Missing token or email. Please open the recovery link from your email.
                </motion.div>
            ) : done ? (
                <motion.div
                    className="mt-4 rounded-2xl bg-emerald-500/10 ring-1 ring-emerald-500/25 px-4 py-3 text-sm text-emerald-100"
                    initial={{ opacity: 0, y: -6 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.25 }}
                >
                    Password updated. You can now{" "}
                    <Link className="text-white/80 hover:text-white underline underline-offset-4" to="/login">
                        sign in
                    </Link>
                    .
                </motion.div>
            ) : (
                <form onSubmit={submit} className="mt-4 grid gap-4">
                    <MotionField
                        label="New password"
                        value={password}
                        onChange={setPassword}
                        type="password"
                        placeholder="••••••••"
                        autoComplete="new-password"
                    />

                    <Button
                        className="h-11 rounded-2xl bg-white text-black hover:bg-white/90"
                        disabled={!canSubmit}
                    >
                        {loading ? "Saving..." : "Change password"}
                    </Button>

                    <div className="text-xs text-white/40 text-center">
                        Minimum 6 characters.
                    </div>
                </form>
            )}
        </AuthCard>
    );
}
