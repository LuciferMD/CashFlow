using System.Text.Json;

namespace AuthTests.Contract;

internal static class ContractPaths
{
    internal static string PactsDirectory =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "pacts"));

    internal static string WebAuthPact =>
        Path.Combine(PactsDirectory, "Web-Auth.json");

    internal static ContractConstants LoadConstants()
    {
        var json = File.ReadAllText(Path.Combine(PactsDirectory, "contract-constants.json"));
        return JsonSerializer.Deserialize<ContractConstants>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        }) ?? throw new InvalidOperationException("Failed to load contract constants.");
    }

    internal sealed class ContractConstants
    {
        public string LoginEmail { get; set; } = string.Empty;
        public string LoginPassword { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }
}
