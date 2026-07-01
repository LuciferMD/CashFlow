export const config = {
  authApiUrl: import.meta.env.VITE_AUTH_API_URL ?? "",
  gatewayApiUrl: import.meta.env.VITE_GATEWAY_API_URL ?? "/graphql",
  notificationHubUrl:
    import.meta.env.VITE_NOTIFICATION_URL ?? "/hubs/notifications",
} as const;
