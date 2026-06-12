export const config = {
  authApiUrl:
    import.meta.env.VITE_AUTH_API_URL ?? "https://localhost:7223",
  gatewayApiUrl:
    import.meta.env.VITE_GATEWAY_API_URL ?? "/graphql",
} as const;
