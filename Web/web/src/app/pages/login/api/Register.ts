const BASE = "https://localhost:7223";
export async function register(userName: string, email: string, password: string) {
    const res = await fetch(`${BASE}/auth/register`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ userName, email, password }),
    });
    if (!res.ok) throw new Error("Registration failed");
}