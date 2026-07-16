import { expect, test } from "@playwright/test";
import { createTestUser } from "../fixtures/test-user.js";
import { AuthPage } from "../pages/auth.page.js";
import { DashboardPage } from "../pages/dashboard.page.js";

test.describe("Auth smoke", () => {
  test("registers a new user and opens the dashboard", async ({ page }) => {
    const user = createTestUser("register");
    const auth = new AuthPage(page);
    const dashboard = new DashboardPage(page);

    await auth.gotoRegister();
    await auth.register(user);

    await dashboard.expectLoaded();
    await dashboard.waitForSensorData();
  });

  test("logs in with valid credentials", async ({ page }) => {
    const user = createTestUser("login");
    const auth = new AuthPage(page);
    const dashboard = new DashboardPage(page);

    await auth.gotoRegister();
    await auth.register(user);

    await dashboard.logout();

    await auth.login(user.email, user.password);
    await dashboard.expectLoaded();
    await dashboard.waitForSensorData();
  });

  test("shows an error for invalid login credentials", async ({ page }) => {
    const auth = new AuthPage(page);

    await auth.gotoLogin();
    await auth.login("unknown@example.com", "wrong-password");

    await expect(page).toHaveURL(/\/$/);
    await expect(page.getByText("Invalid email or password")).toBeVisible();
  });
});
