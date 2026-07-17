import { config } from "../../../../config/api.ts";

const BASE = config.authApiUrl;

export async function login(
  email: string,
  password: string,
  baseUrl: string = BASE,
) {
    const res = await fetch(`${baseUrl}/auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include", // sends/receives cookies
        body: JSON.stringify({ email, password }),
    });

    return res.ok;
}