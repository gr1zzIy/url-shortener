import { useLocation, useNavigate, isRouteErrorResponse, useRouteError } from "react-router-dom";
import { Button } from "@/components/ui/button";

type ErrorState = { title?: string; message?: string; code?: number };

export default function ErrorPage() {
    const nav = useNavigate();
    const loc = useLocation();

    // Works in two cases:
    // 1) You navigate("/error", { state: ... })
    // 2) Router renders this as errorElement and provides route error
    const routeErr = useRouteError();
    const state = (loc.state ?? {}) as ErrorState;

    let title = state.title ?? "Something went wrong";
    let message =
        state.message ??
        "We couldn’t complete your request. Please try again. If the issue persists, come back later.";
    let code = state.code;

    if (routeErr) {
        if (isRouteErrorResponse(routeErr)) {
            code = routeErr.status;
            title = `Request failed (${routeErr.status})`;
            message = routeErr.statusText || message;
        } else if (routeErr instanceof Error) {
            title = "Unexpected error";
            message = routeErr.message || message;
        }
    }

    return (
        <div className="mx-auto max-w-3xl px-4 py-16 text-white">
            <div className="glass-surface p-8">
                <div className="text-xs text-white/60">Error</div>
                <h1 className="mt-2 text-2xl font-semibold">{title}</h1>

                {code ? (
                    <div className="mt-2 text-sm text-white/60">
                        Code: <span className="text-white/80">{code}</span>
                    </div>
                ) : null}

                <p className="mt-4 text-white/70">{message}</p>

                <div className="mt-6 flex flex-wrap gap-3">
                    <Button className="rounded-2xl bg-white text-black hover:bg-white/90" onClick={() => nav(0)}>
                        Retry
                    </Button>
                    <Button
                        variant="secondary"
                        className="rounded-2xl bg-white/10 text-white hover:bg-white/15"
                        onClick={() => nav("/dashboard")}
                    >
                        Go to Dashboard
                    </Button>
                    <Button
                        variant="secondary"
                        className="rounded-2xl bg-white/10 text-white hover:bg-white/15"
                        onClick={() => nav("/login")}
                    >
                        Login
                    </Button>
                </div>

                <div className="mt-6 text-xs text-white/40 break-words">Path: {loc.pathname}</div>
            </div>
        </div>
    );
}
