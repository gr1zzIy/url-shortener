import { createBrowserRouter } from "react-router-dom";
import ErrorPage from "@/pages/ErrorPage";
import NotFoundPage from "@/pages/NotFoundPage";

import LoginPage from "@/pages/LoginPage";
import RegisterPage from "@/pages/RegisterPage";
import DashboardPage from "@/pages/DashboardPage"; // твій
import { ProtectedRoute } from "@/routes/ProtectedRoute";

export const router = createBrowserRouter([
    {
        path: "/",
        errorElement: <ErrorPage />, // runtime errors in routes
        children: [
            { index: true, element: <LoginPage /> }, // або Landing/Home
            { path: "login", element: <LoginPage /> },
            { path: "register", element: <RegisterPage /> },

            {
                path: "dashboard",
                element: (
                    <ProtectedRoute>
                        <DashboardPage />
                    </ProtectedRoute>
                ),
            },

            // explicit API error route (when backend returned JSON error etc.)
            { path: "error", element: <ErrorPage /> },

            { path: "*", element: <NotFoundPage /> },
        ],
    },
]);
