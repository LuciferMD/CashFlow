import { expect, type Page } from "@playwright/test";
import type { TestUser } from "../fixtures/test-user.js";

export class AuthPage {
  constructor(private readonly page: Page) {}

  async gotoLogin(): Promise<void> {
    await this.page.goto("/");
    await expect(this.page.getByRole("heading", { name: "SensorHub" })).toBeVisible();
  }

  async gotoRegister(): Promise<void> {
    await this.page.goto("/register");
    await expect(
      this.page.getByText("Create your monitoring account"),
    ).toBeVisible();
  }

  async login(email: string, password: string): Promise<void> {
    await this.page.getByLabel("Email").fill(email);
    await this.page.getByLabel("Password").fill(password);
    await this.page.getByRole("button", { name: "Sign In" }).click();
  }

  async register(user: TestUser): Promise<void> {
    await this.page.getByLabel("Full Name").fill(user.name);
    await this.page.getByLabel("Email").fill(user.email);
    await this.page.getByLabel("Password", { exact: true }).fill(user.password);
    await this.page.getByLabel("Confirm Password").fill(user.password);
    await this.page.getByRole("button", { name: "Create Account" }).click();
  }
}
