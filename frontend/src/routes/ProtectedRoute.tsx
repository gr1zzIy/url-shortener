import { Navigate } from "react-router-dom";
import { authStore } from "../auth/authStore";
import type { ReactNode } from "react";

export function ProtectedRoute({ children }: { children: ReactNode }) {
    const token = authStore.getAccessToken();
    if (!token) return <Navigate to="/login" replace />;
    return <>{children}</>;
}