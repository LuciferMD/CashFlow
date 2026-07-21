import { vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router";

// Suppress motion animation in AnimatedSensor
vi.mock("motion/react", () => ({
  motion: new Proxy({} as Record<string, string>, { get: (_t, tag: string) => tag }),
}));

// Mock the register API so we control the response without hitting the network
vi.mock("../../app/pages/login/api/Register", () => ({
  register: vi.fn(),
}));

import { RegisterPage } from "../../app/pages/login/ui/RegisterPage";
import { register } from "../../app/pages/login/api/Register";
const mockRegister = vi.mocked(register);

function renderPage() {
  return render(
    <MemoryRouter initialEntries={["/register"]}>
      <RegisterPage />
    </MemoryRouter>,
  );
}

describe("RegisterPage", () => {
  beforeEach(() => {
    mockRegister.mockReset();
  });

  it("renders all four form fields", () => {
    renderPage();
    expect(screen.getByLabelText(/full name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    // two password fields — match by the more specific label text
    expect(screen.getByLabelText(/^password$/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/confirm password/i)).toBeInTheDocument();
  });

  it("shows an error when the passwords do not match (client-side check)", async () => {
    renderPage();
    const user = userEvent.setup();

    await user.type(screen.getByLabelText(/full name/i), "Alice");
    await user.type(screen.getByLabelText(/email/i), "alice@example.com");
    await user.type(screen.getByLabelText(/^password$/i), "secret1");
    await user.type(screen.getByLabelText(/confirm password/i), "secret2");
    await user.click(screen.getByRole("button", { name: /create account/i }));

    await waitFor(() => {
      expect(screen.getByText(/passwords do not match/i)).toBeInTheDocument();
    });
    // register API should not be called in this case
    expect(mockRegister).not.toHaveBeenCalled();
  });

  it("shows an error when the register API returns a failure result", async () => {
    mockRegister.mockResolvedValue({
      ok: false,
      message: "Something went wrong. Please try again",
    });
    const user = userEvent.setup();
    renderPage();

    await user.type(screen.getByLabelText(/full name/i), "Bob");
    await user.type(screen.getByLabelText(/email/i), "bob@example.com");
    await user.type(screen.getByLabelText(/^password$/i), "pass123");
    await user.type(screen.getByLabelText(/confirm password/i), "pass123");
    await user.click(screen.getByRole("button", { name: /create account/i }));

    await waitFor(() => {
      expect(screen.getByText(/something went wrong/i)).toBeInTheDocument();
    });
  });

  it("calls register with name, email and password on a valid submission", async () => {
    mockRegister.mockResolvedValue({ ok: true });
    const user = userEvent.setup();
    renderPage();

    await user.type(screen.getByLabelText(/full name/i), "Charlie");
    await user.type(screen.getByLabelText(/email/i), "charlie@example.com");
    await user.type(screen.getByLabelText(/^password$/i), "strongpass");
    await user.type(screen.getByLabelText(/confirm password/i), "strongpass");
    await user.click(screen.getByRole("button", { name: /create account/i }));

    await waitFor(() => {
      expect(mockRegister).toHaveBeenCalledWith("Charlie", "charlie@example.com", "strongpass");
    });
  });

  it("shows a server-error message when the register API rejects", async () => {
    mockRegister.mockRejectedValue(new Error("Server down"));
    const user = userEvent.setup();
    renderPage();

    await user.type(screen.getByLabelText(/full name/i), "Dan");
    await user.type(screen.getByLabelText(/email/i), "dan@example.com");
    await user.type(screen.getByLabelText(/^password$/i), "pass");
    await user.type(screen.getByLabelText(/confirm password/i), "pass");
    await user.click(screen.getByRole("button", { name: /create account/i }));

    await waitFor(() => {
      expect(screen.getByText(/could not reach the server/i)).toBeInTheDocument();
    });
  });
});
