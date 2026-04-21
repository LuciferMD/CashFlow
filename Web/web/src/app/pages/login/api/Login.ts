import { config } from "../../../../config/api.ts";

const BASE = config.authApiUrl;
export async function login(email: string, password: string) {
    const res = await fetch(`${BASE}/auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include", // sends/receives cookies
        body: JSON.stringify({ email, password }),
    });

    return res.ok;
}