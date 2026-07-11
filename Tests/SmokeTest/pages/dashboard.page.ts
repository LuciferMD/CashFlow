import { expect, type Page } from "@playwright/test";

export class DashboardPage {
  constructor(private readonly page: Page) {}

  async expectLoaded(): Promise<void> {
    await expect(this.page).toHaveURL(/\/dashboard$/);
    await expect(
      this.page.getByRole("heading", { name: "Home sensor overview" }),
    ).toBeVisible();
    await expect(this.page.getByRole("tab", { name: "Current data" })).toBeVisible();
  }

  async waitForDashboardReady(): Promise<void> {
    await this.expectLoaded();

    const refreshButton = this.page.getByRole("button", { name: "Refresh" });
    await expect(refreshButton).toBeEnabled({ timeout: 45_000 });
  }

  async waitForSensorData(): Promise<void> {
    await this.waitForDashboardReady();
    await expect(
      this.page.getByText("Updated", { exact: false }).first(),
    ).toBeVisible({ timeout: 45_000 });
  }

  async logout(): Promise<void> {
    await this.page.getByRole("button", { name: "Logout" }).click();
    await expect(this.page).toHaveURL(/\/$/);
    await expect(this.page.getByRole("button", { name: "Sign In" })).toBeVisible();
  }
}
