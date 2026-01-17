import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { AuthCard } from "../ui/AuthCard";
import { Button } from "@/components/ui/button";
import { MotionField } from "../ui/MotionField";
import { api } from "../api/client";
import { motion } from "framer-motion";
import { ApiError } from "../api/http";

export default function ForgotPasswordPage() {
    const nav = useNavigate();

    const [email, setEmail] = useState("");
    const [sent, setSent] = useState(false);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");

    const submit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError("");

        try {
            setLoading(true);
            await api.auth.forgotPassword(email);
            setSent(true);
        } catch (err) {
            if (err instanceof ApiError) setError(err.problem?.title ?? err.message);
            else setError("Unexpected error");
        } finally {
            setLoading(false);
        }
    };

    return (
        <AuthCard
            variant="single"
            title="Forgot password"
            subtitle="We will send a recovery link if the email exists."
        >
            <div className="flex items-center justify-between">
                <button
                    type="button"
                    onClick={() => nav(-1)}
                    className="text-xs text-white/60 hover:text-white underline underline-offset-4 transition-colors"
                >
                    ← Back
                </button>

                <Link
                    to="/login"
                    className="text-xs text-white/60 hover:text-white underline underline-offset-4 transition-colors"
                >
                    Back to login
                </Link>
            </div>

            {sent ? (
                <motion.div
                    className="mt-4 rounded-2xl border border-white/10 bg-black/20 p-4"
                    initial={{ opacity: 0, y: 8 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.22 }}
                >
                    <div className="text-sm font-semibold text-white">Check your email</div>
                    <div className="mt-1 text-xs text-white/60 leading-relaxed">
                        If an account exists for <span className="text-white/80">{email}</span>, we sent a recovery link.
                    </div>

                    <div className="mt-4 flex flex-col gap-2 sm:flex-row">
                        <Button
                            className="h-11 rounded-2xl bg-white text-black hover:bg-white/90"
                            onClick={() => setSent(false)}
                        >
                            Send again
                        </Button>
                        <Button
                            variant="secondary"
                            className="h-11 rounded-2xl bg-white/10 text-white hover:bg-white/15"
                            asChild
                        >
                            <Link to="/login">Go to login</Link>
                        </Button>
                    </div>

                    <div className="mt-3 text-xs text-white/40">
                        Didn’t get it? Check spam or try another email.
                    </div>
                </motion.div>
            ) : (
                <form onSubmit={submit} className="mt-4 grid gap-4">
                    <MotionField
                        label="Email"
                        value={email}
                        onChange={setEmail}
                        placeholder="user@example.com"
                        inputMode="email"
                        autoComplete="email"
                    />

                    {error && (
                        <motion.div
                            key={error}
                            className="rounded-2xl bg-red-500/10 ring-1 ring-red-500/30 px-4 py-3 text-sm text-red-200"
                            initial={{ opacity: 0, y: -6 }}
                            animate={{ opacity: 1, y: 0 }}
                            transition={{ duration: 0.2 }}
                        >
                            {error}
                        </motion.div>
                    )}

                    <Button
                        className="h-11 rounded-2xl bg-white text-black hover:bg-white/90"
                        disabled={!email || loading}
                    >
                        {loading ? "Sending..." : "Send recovery link"}
                    </Button>

                    <div className="text-xs text-white/40 text-center">
                        Tip: you can return to{" "}
                        <Link className="text-white/70 hover:text-white underline underline-offset-4" to="/login">
                            Login
                        </Link>
                    </div>
                </form>
            )}
        </AuthCard>
    );
}
