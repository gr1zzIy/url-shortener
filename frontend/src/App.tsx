import { createBrowserRouter, RouterProvider } from "react-router-dom";
import AppShell from "./pages/AppShell";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import DashboardPage from "./pages/DashboardPage";
import ErrorPage from "./pages/ErrorPage";
import NotFoundPage from "./pages/NotFoundPage";
import { ProtectedRoute } from "./routes/ProtectedRoute";
import ForgotPasswordPage from "./pages/ForgotPasswordPage";
import ResetPasswordPage from "./pages/ResetPasswordPage";

const router = createBrowserRouter([
    // Public
    { path: "/login", element: <LoginPage />, errorElement: <ErrorPage /> },
    { path: "/register", element: <RegisterPage />, errorElement: <ErrorPage /> },
    { path: "/forgot-password", element: <ForgotPasswordPage />, errorElement: <ErrorPage /> },
    { path: "/reset-password", element: <ResetPasswordPage />, errorElement: <ErrorPage /> },

    // Dedicated error route (for API failures you catch and redirect)
    { path: "/error", element: <ErrorPage /> },

    // App shell
    {
        path: "/",
        element: <AppShell />,
        errorElement: <ErrorPage />,
        children: [
            { index: true, element: <ProtectedRoute><DashboardPage /></ProtectedRoute> },
            { path: "dashboard", element: <ProtectedRoute><DashboardPage /></ProtectedRoute> },
        ],
    },

    // 404
    { path: "*", element: <NotFoundPage /> },
]);

export default function App() {
    return <RouterProvider router={router} />;
}
