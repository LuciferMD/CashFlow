import { config } from "../../../../config/api.ts";

const BASE = config.authApiUrl;

export type RegisterResult =
  | { ok: true }
  | { ok: false; message: string };

export async function register(
  userName: string,
  email: string,
  password: string,
  baseUrl: string = BASE,
): Promise<RegisterResult> {
  const res = await fetch(`${baseUrl}/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify({ userName, email, password }),
  });

  if (res.ok) {
    return { ok: true };
  }

  if (res.status === 409) {
    const body = (await res.json().catch(() => null)) as { message?: string } | null;
    return { ok: false, message: body?.message ?? "Email already registered" };
  }

  return { ok: false, message: "Something went wrong. Please try again" };
}
