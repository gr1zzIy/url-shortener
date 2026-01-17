import { Link } from "react-router-dom";
import { Button } from "@/components/ui/button";

export default function NotFoundPage() {
    return (
        <div className="mx-auto max-w-3xl px-4 py-16 text-white">
            <div className="glass-surface p-8">
                <div className="text-xs text-white/60">404</div>
                <h1 className="mt-2 text-2xl font-semibold">Page not found</h1>
                <p className="mt-4 text-white/70">This page doesn’t exist or was moved.</p>

                <div className="mt-6 flex gap-3">
                    <Button asChild className="rounded-2xl bg-white text-black hover:bg-white/90">
                        <Link to="/dashboard">Go to Dashboard</Link>
                    </Button>
                    <Button asChild variant="secondary" className="rounded-2xl bg-white/10 text-white hover:bg-white/15">
                        <Link to="/login">Login</Link>
                    </Button>
                </div>
            </div>
        </div>
    );
}
