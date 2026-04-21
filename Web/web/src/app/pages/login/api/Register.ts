import {config} from "../../../../config/api.ts";

const BASE = config.authApiUrl;

export async function register(userName: string, email: string, password: string) {

    const res = await fetch(`${BASE}/auth/register`, {
        method: "POST",
        headers: {"Content-Type": "application/json"},
        body: JSON.stringify({userName, email, password}),
    })

    return res.ok;
}