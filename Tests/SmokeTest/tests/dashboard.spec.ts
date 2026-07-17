import { test, expect } from "../fixtures/dashboard.js";

test.describe("Dashboard smoke", () => {
  test("shows live sensor overview after authentication", async ({
    page,
    dashboard,
  }) => {
    await expect(page.getByText("Live monitoring")).toBeVisible();
    await expect(page.getByRole("tab", { name: "Current data" })).toHaveAttribute(
      "data-state",
      "active",
    );
    await dashboard.expectLoaded();
  });

  test("logs out back to the login page", async ({ dashboard }) => {
    await dashboard.logout();
  });
});
