import { vi } from "vitest";
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router";

// Suppress motion animation in AnimatedSensor
vi.mock("motion/react", () => ({
  motion: new Proxy({} as Record<string, string>, { get: (_t, tag: string) => tag }),
}));

// Mock the login API so we control the response without hitting the network
vi.mock("../../app/pages/login/api/Login", () => ({
  login: vi.fn(),
}));

import { LoginPage } from "../../app/pages/login/ui/LoginPage";
import { login } from "../../app/pages/login/api/Login";
const mockLogin = vi.mocked(login);

function renderPage() {
  return render(
    <MemoryRouter initialEntries={["/"]}>
      <LoginPage />
    </MemoryRouter>,
  );
}

describe("LoginPage", () => {
  beforeEach(() => {
    mockLogin.mockReset();
  });

  it("renders email and password fields", () => {
    renderPage();
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
  });

  it("renders the Sign In button", () => {
    renderPage();
    expect(screen.getByRole("button", { name: /sign in/i })).toBeInTheDocument();
  });

  it("shows the 'Sign up' navigation link", () => {
    renderPage();
    expect(screen.getByRole("button", { name: /sign up/i })).toBeInTheDocument();
  });

  it("shows an error message when login returns false", async () => {
    mockLogin.mockResolvedValue(false);
    const user = userEvent.setup();
    renderPage();

    await user.type(screen.getByLabelText(/email/i), "bad@user.com");
    await user.type(screen.getByLabelText(/password/i), "wrongpass");
    await user.click(screen.getByRole("button", { name: /sign in/i }));

    await waitFor(() => {
      expect(screen.getByText(/invalid email or password/i)).toBeInTheDocument();
    });
  });

  it("shows a generic error when the login API rejects", async () => {
    mockLogin.mockRejectedValue(new Error("Network failure"));
    const user = userEvent.setup();
    renderPage();

    await user.type(screen.getByLabelText(/email/i), "a@b.com");
    await user.type(screen.getByLabelText(/password/i), "pass");
    fireEvent.submit(screen.getByRole("button", { name: /sign in/i }).closest("form")!);

    await waitFor(() => {
      expect(screen.getByText(/something went wrong/i)).toBeInTheDocument();
    });
  });

  it("calls login with the entered email and password on submit", async () => {
    mockLogin.mockResolvedValue(true);
    const user = userEvent.setup();
    renderPage();

    await user.type(screen.getByLabelText(/email/i), "user@example.com");
    await user.type(screen.getByLabelText(/password/i), "correctpass");
    await user.click(screen.getByRole("button", { name: /sign in/i }));

    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith("user@example.com", "correctpass");
    });
  });
});
