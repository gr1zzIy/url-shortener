import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";

type GlassTheme = "dark" | "balanced" | "light";
const KEY = "glassTheme";

function applyTheme(t: GlassTheme) {
    document.documentElement.setAttribute("data-glass", t);
}

export function GlassThemeSwitch() {
    const [theme, setTheme] = useState<GlassTheme>("balanced");

    useEffect(() => {
        const saved = (localStorage.getItem(KEY) as GlassTheme | null) ?? "balanced";
        setTheme(saved);
        applyTheme(saved);
    }, []);

    const set = (t: GlassTheme) => {
        setTheme(t);
        localStorage.setItem(KEY, t);
        applyTheme(t);
    };

    return (
        <div className="inline-flex items-center gap-2 rounded-2xl border border-white/10 bg-white/5 px-2 py-2 backdrop-blur-xl">
            <Button
                variant="ghost"
                onClick={() => set("dark")}
                className={theme === "dark" ? "bg-white/10" : ""}
            >
                Dark
            </Button>
            <Button
                variant="ghost"
                onClick={() => set("balanced")}
                className={theme === "balanced" ? "bg-white/10" : ""}
            >
                Balanced
            </Button>
            <Button
                variant="ghost"
                onClick={() => set("light")}
                className={theme === "light" ? "bg-white/10" : ""}
            >
                Light
            </Button>
        </div>
    );
}
