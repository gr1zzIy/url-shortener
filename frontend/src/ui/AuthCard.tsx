import type { ReactNode } from "react";
import { motion } from "framer-motion";
import type { Variants } from "framer-motion";
import { Logo } from "./Logo";

const container: Variants = {
    hidden: { opacity: 0 },
    show: {
        opacity: 1,
        transition: { staggerChildren: 0.08, delayChildren: 0.05 },
    },
};

const item: Variants = {
    hidden: { opacity: 0, y: 12, filter: "blur(6px)" },
    show: {
        opacity: 1,
        y: 0,
        filter: "blur(0px)",
        transition: { type: "spring" as const, stiffness: 420, damping: 34 },
    },
};

type Props = {
    title: string;
    subtitle: string;
    children: ReactNode;

    // existing
    leftExtra?: ReactNode;

    // new
    variant?: "split" | "single"; // single = form only centered (best for forgot/reset)
    aside?: ReactNode; // custom left side content
};

function MiniInfo({ title, body }: { title: string; body: string }) {
    return (
        <div className="rounded-2xl bg-black/20 ring-1 ring-white/10 p-4">
            <div className="text-sm font-medium text-white">{title}</div>
            <div className="mt-1 text-xs text-white/60 leading-relaxed">{body}</div>
        </div>
    );
}

function DefaultAside() {
    return (
        <div className="grid gap-3">
            <MiniInfo title="Control" body="Deactivate links, set expirations, keep things tidy." />
            <MiniInfo title="Analytics" body="Track clicks, unique visitors, and trends over time." />
            <MiniInfo title="Safety" body="Consistent validation and predictable error handling." />
            <div className="pt-2 text-xs text-white/40">
                Tip: press <span className="text-white/70">Tab</span> to jump between fields
            </div>
        </div>
    );
}

export function AuthCard({
                             title,
                             subtitle,
                             children,
                             leftExtra,
                             variant = "split",
                             aside,
                         }: Props) {
    const showAside = variant === "split";

    return (
        <div className="min-h-screen text-white">
            <div className="mx-auto max-w-6xl px-4 py-10">
                <div className="mb-8 flex items-center justify-between">
                    <Logo />
                    <div className="hidden sm:block text-xs text-white/35">Glass UI · production-style UX</div>
                </div>

                <motion.div
                    variants={container}
                    initial="hidden"
                    animate="show"
                    className={[
                        "grid gap-6 items-start",
                        showAside ? "lg:grid-cols-[1fr_1fr]" : "",
                    ].join(" ")}
                >
                    {/* LEFT */}
                    {showAside ? (
                        <motion.div variants={item} className="glass-surface p-7 min-w-0">
                            <div className="text-xs text-white/60">Welcome</div>
                            <h1 className="mt-1 text-2xl font-semibold tracking-tight">{title}</h1>
                            <p className="mt-2 text-sm text-white/70 leading-relaxed">{subtitle}</p>

                            <div className="mt-6">
                                {aside ?? <DefaultAside />}
                            </div>

                            {leftExtra ? (
                                <motion.div variants={item} className="mt-6">
                                    {leftExtra}
                                </motion.div>
                            ) : null}
                        </motion.div>
                    ) : null}

                    {/* RIGHT (form) */}
                    <motion.div
                        variants={item}
                        className={[
                            "glass-surface p-7 min-w-0",
                            showAside
                                ? ""
                                : "mx-auto w-full max-w-[560px]", // center for single mode
                        ].join(" ")}
                    >
                        {children}
                    </motion.div>
                </motion.div>

                <div className="mt-8 text-center text-xs text-white/30">
                    GlassLink · portfolio build
                </div>
            </div>
        </div>
    );
}