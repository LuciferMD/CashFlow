import { test as base } from "@playwright/test";
import { DashboardPage } from "../pages/dashboard.page.js";

export const test = base.extend({
  dashboard: async ({ page }, use) => {
    const dashboard = new DashboardPage(page);

    await page.goto("/dashboard");
    await dashboard.waitForDashboardReady();

    await use(dashboard);
  },
});

export { expect } from "@playwright/test";
