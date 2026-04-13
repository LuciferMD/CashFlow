import { createBrowserRouter } from "react-router";
import { LoginPage } from "./pages/login/ui/LoginPage.tsx";
import { RegisterPage } from "./pages/login/ui/RegisterPage.tsx";
import { DashboardPage } from "./pages/DashboardPage.tsx";

export const router = createBrowserRouter([
  {
    path: "/",
    Component: LoginPage,
  },
  {
    path: "/register",
    Component: RegisterPage,
  },
  {
    path: "/dashboard",
    Component: DashboardPage,
  },
]);
