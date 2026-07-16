import {config} from "../../../../config/api.ts";

const BASE = config.authApiUrl;

export async function register(
  userName: string,
  email: string,
  password: string,
  baseUrl: string = BASE,
) {

    const res = await fetch(`${baseUrl}/auth/register`, {
        method: "POST",
        headers: {"Content-Type": "application/json"},
        credentials: "include",
        body: JSON.stringify({userName, email, password}),
    })

    return res.ok;
}