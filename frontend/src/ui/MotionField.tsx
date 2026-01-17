import { useMemo, useState } from "react";
import { motion } from "framer-motion";
import { Input } from "@/components/ui/input";

type Props = {
    label: string;
    value: string;
    onChange: (v: string) => void;
    type?: string;
    placeholder?: string;
    autoComplete?: string;
    inputMode?: React.HTMLAttributes<HTMLInputElement>["inputMode"];
    rightAdornment?: React.ReactNode;
};

export function MotionField({
                                label,
                                value,
                                onChange,
                                type = "text",
                                placeholder,
                                autoComplete,
                                inputMode,
                                rightAdornment,
                            }: Props) {
    const [focused, setFocused] = useState(false);
    const isFloating = useMemo(() => focused || value.length > 0, [focused, value]);

    return (
        <div className="grid gap-2">
            <div className="relative">
                {/* Highlight sweep layer */}
                <motion.span
                    className="pointer-events-none absolute -inset-[1px] rounded-2xl opacity-0"
                    animate={{ opacity: focused ? 1 : 0 }}
                    transition={{ duration: 0.15 }}
                >
                    <motion.span
                        className="absolute inset-0 rounded-2xl"
                        style={{
                            background:
                                "linear-gradient(90deg, rgba(255,255,255,0), rgba(255,255,255,0.22), rgba(255,255,255,0))",
                            filter: "blur(6px)",
                        }}
                        initial={false}
                        animate={{ x: focused ? ["-60%", "60%"] : "0%" }}
                        transition={{ duration: 0.75, ease: "easeInOut" }}
                    />
                </motion.span>

                <div
                    className={[
                        "rounded-2xl p-[1px] transition-all duration-200",
                        focused ? "bg-white/25 shadow-[0_0_0_4px_rgba(255,255,255,0.08)]" : "bg-white/10",
                    ].join(" ")}
                >
                    <div className="relative">
                        {/* Floating label */}
                        <motion.label
                            className="absolute left-4 text-white/60 pointer-events-none"
                            initial={false}
                            animate={{
                                top: isFloating ? 8 : 14,
                                scale: isFloating ? 0.85 : 1,
                                opacity: isFloating ? 0.9 : 0.7,
                            }}
                            transition={{ type: "spring", stiffness: 450, damping: 35 }}
                            style={{ transformOrigin: "left top" }}
                        >
                            {label}
                        </motion.label>

                        <Input
                            className={[
                                "h-12 rounded-2xl bg-black/25 border-white/10 text-white placeholder:text-white/40",
                                "pt-6",
                                rightAdornment ? "pr-14" : "",
                                "transition-all duration-200",
                                focused ? "bg-black/35" : "",
                            ].join(" ")}
                            value={value}
                            onChange={(e) => onChange(e.target.value)}
                            onFocus={() => setFocused(true)}
                            onBlur={() => setFocused(false)}
                            type={type}
                            placeholder={isFloating ? placeholder : ""}
                            autoComplete={autoComplete}
                            inputMode={inputMode}
                        />

                        {rightAdornment ? (
                            <div className="absolute right-3 top-1/2 -translate-y-1/2">{rightAdornment}</div>
                        ) : null}
                    </div>
                </div>
            </div>
        </div>
    );
}
