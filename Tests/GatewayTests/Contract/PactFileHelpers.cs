using System.Text.Json;

namespace GatewayTests.Contract;

internal static class PactFileHelpers
{
    internal static string PatchBearerToken(string pactPath, string jwt)
    {
        var json = File.ReadAllText(pactPath);
        var patched = json.Replace("contract-test-jwt", jwt, StringComparison.Ordinal);
        var tempPath = Path.Combine(Path.GetTempPath(), $"web-gateway-pact-{Guid.NewGuid():N}.json");
        File.WriteAllText(tempPath, patched);
        return tempPath;
    }
}
