const DEFAULT_BASE_URL = "https://localhost:3000";

export default async function globalSetup(): Promise<void> {
  const baseURL = process.env.E2E_BASE_URL ?? DEFAULT_BASE_URL;

  // Vite dev server uses a local self-signed certificate.
  process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";

  try {
    const response = await fetch(baseURL, {
      signal: AbortSignal.timeout(5_000),
    });

    if (!response.ok && response.status !== 404) {
      throw new Error(`Unexpected status ${response.status}`);
    }
  } catch (error) {
    const message =
      error instanceof Error ? error.message : "Unknown connection error";

    throw new Error(
      [
        `Cannot reach the web app at ${baseURL} (${message}).`,
        "Start the stack before running smoke tests:",
        "  docker compose up -d",
        "  # or: cd Web/web && npm run dev",
        "Then set E2E_BASE_URL if needed (e.g. https://localhost:3000 for Docker web).",
      ].join("\n"),
    );
  }
}
