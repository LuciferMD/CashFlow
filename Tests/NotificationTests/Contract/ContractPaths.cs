namespace NotificationTests.Contract;

internal static class ContractPaths
{
    internal static string PactsDirectory =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "pacts"));

    internal static string NotificationGatewayPact =>
        Path.Combine(PactsDirectory, "Notification-Gateway.json");
}
