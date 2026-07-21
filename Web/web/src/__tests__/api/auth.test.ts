import { vi } from "vitest";
import { login } from "../../app/pages/login/api/Login";
import { register } from "../../app/pages/login/api/Register";

const mockFetch = vi.fn();

beforeEach(() => {
  vi.stubGlobal("fetch", mockFetch);
  mockFetch.mockReset();
});

afterEach(() => {
  vi.unstubAllGlobals();
});

// ─── login ──────────────────────────────────────────────────────────────────

describe("login", () => {
  it("returns true when the server responds with 200 OK", async () => {
    mockFetch.mockResolvedValue({ ok: true });
    expect(await login("user@example.com", "s3cret")).toBe(true);
  });

  it("returns false when the server responds with a non-OK status", async () => {
    mockFetch.mockResolvedValue({ ok: false });
    expect(await login("user@example.com", "wrong")).toBe(false);
  });

  it("sends credentials as JSON in the request body", async () => {
    mockFetch.mockResolvedValue({ ok: true });
    await login("a@b.com", "pass");

    const [, options] = mockFetch.mock.calls[0] as [string, RequestInit];
    const body = JSON.parse(options.body as string) as { email: string; password: string };
    expect(body.email).toBe("a@b.com");
    expect(body.password).toBe("pass");
  });
});

// ─── register ───────────────────────────────────────────────────────────────

describe("register", () => {
  it("returns ok when the server responds with 200 OK", async () => {
    mockFetch.mockResolvedValue({ ok: true });
    expect(await register("Alice", "alice@example.com", "pass123")).toEqual({ ok: true });
  });

  it("returns an error result when the server responds with a non-OK status", async () => {
    mockFetch.mockResolvedValue({ ok: false, status: 500, json: async () => ({}) });
    expect(await register("Bob", "bob@example.com", "pass123")).toEqual({
      ok: false,
      message: "Something went wrong. Please try again",
    });
  });

  it("sends userName, email and password in the request body", async () => {
    mockFetch.mockResolvedValue({ ok: true });
    await register("Charlie", "c@c.com", "mypassword");

    const [, options] = mockFetch.mock.calls[0] as [string, RequestInit];
    const body = JSON.parse(options.body as string) as {
      userName: string;
      email: string;
      password: string;
    };
    expect(body.userName).toBe("Charlie");
    expect(body.email).toBe("c@c.com");
    expect(body.password).toBe("mypassword");
  });
});
