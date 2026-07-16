import { test as setup } from "@playwright/test";
import { createTestUser } from "../fixtures/test-user.js";
import { AuthPage } from "../pages/auth.page.js";
import { DashboardPage } from "../pages/dashboard.page.js";

const authFile = ".auth/smoke-user.json";

setup("prepare authenticated session", async ({ page }) => {
  const user = createTestUser("session");
  const auth = new AuthPage(page);
  const dashboard = new DashboardPage(page);

  await auth.gotoRegister();
  await auth.register(user);
  await dashboard.expectLoaded();
  await dashboard.waitForSensorData();

  await page.context().storageState({ path: authFile });
});
