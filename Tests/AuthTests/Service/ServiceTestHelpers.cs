using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AuthTests.Service;

internal static class ServiceTestHelpers
{
    internal static HttpClient CreateClient(WebApplicationFactory<Program> factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
        });

    internal static object BuildRegisterRequest(
        string? userName = null,
        string? email = null,
        string? password = null) =>
        new
        {
            userName = userName ?? $"user-{Guid.NewGuid():N}",
            email = email ?? $"user-{Guid.NewGuid():N}@example.com",
            password = password ?? "SecurePass1!",
        };

    internal static object BuildLoginRequest(string email, string password) =>
        new { email, password };

    internal static async Task<HttpResponseMessage> RegisterAsync(
        HttpClient client,
        string? userName = null,
        string? email = null,
        string? password = null)
    {
        var body = BuildRegisterRequest(userName, email, password);
        return await client.PostAsJsonAsync("/auth/register", body);
    }

    internal static async Task<(HttpResponseMessage Response, string Email, string Password)> RegisterUniqueUserAsync(
        HttpClient client)
    {
        var email = $"user-{Guid.NewGuid():N}@example.com";
        const string password = "SecurePass1!";
        var response = await RegisterAsync(client, email: email, password: password);
        return (response, email, password);
    }

    internal static async Task<HttpResponseMessage> LoginAsync(
        HttpClient client,
        string email,
        string password) =>
        await client.PostAsJsonAsync("/auth/login", BuildLoginRequest(email, password));

    internal static bool HasGuardPassCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            return false;
        }

        return cookies.Any(cookie => cookie.StartsWith("GuardPass=", StringComparison.OrdinalIgnoreCase));
    }

    internal static async Task<string> ReadBodyAsync(HttpResponseMessage response) =>
        await response.Content.ReadAsStringAsync();
}
