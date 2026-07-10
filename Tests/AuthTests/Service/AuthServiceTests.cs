using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AuthTests.Service;

/// <summary>
/// Service-level tests for the Auth API.
/// Uses WebApplicationFactory (in-process host) + Testcontainers PostgreSQL.
/// Only the Auth service is exercised — no Gateway, Notification, or HistoryStore.
/// </summary>
[Collection(nameof(AuthServiceCollection))]
[Trait("Category", "Service")]
public sealed class AuthServiceTests
{
    private readonly AuthServiceFixture _fixture;

    public AuthServiceTests(AuthServiceFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Register_WithValidBody_ReturnsOkAndSetsGuardPassCookie()
    {
        var client = ServiceTestHelpers.CreateClient(_fixture.Factory);

        var response = await ServiceTestHelpers.RegisterAsync(client);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        ServiceTestHelpers.HasGuardPassCookie(response).Should().BeTrue();
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsOkAndSetsGuardPassCookie()
    {
        var client = ServiceTestHelpers.CreateClient(_fixture.Factory);
        var (registerResponse, email, password) =
            await ServiceTestHelpers.RegisterUniqueUserAsync(client);
        registerResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var loginResponse = await ServiceTestHelpers.LoginAsync(client, email, password);

        loginResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        ServiceTestHelpers.HasGuardPassCookie(loginResponse).Should().BeTrue();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var client = ServiceTestHelpers.CreateClient(_fixture.Factory);
        var (_, email, _) = await ServiceTestHelpers.RegisterUniqueUserAsync(client);

        var response = await ServiceTestHelpers.LoginAsync(client, email, "WrongPassword!");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        ServiceTestHelpers.HasGuardPassCookie(response).Should().BeFalse();
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        var client = ServiceTestHelpers.CreateClient(_fixture.Factory);

        var response = await ServiceTestHelpers.LoginAsync(
            client,
            "nobody@example.com",
            "AnyPassword1!");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        ServiceTestHelpers.HasGuardPassCookie(response).Should().BeFalse();
    }

    [Fact]
    public async Task GetAuth_WithValidJwt_ReturnsOk()
    {
        var client = ServiceTestHelpers.CreateClient(_fixture.Factory);
        var (registerResponse, _, _) = await ServiceTestHelpers.RegisterUniqueUserAsync(client);
        registerResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var response = await client.GetAsync("/auth");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await ServiceTestHelpers.ReadBodyAsync(response);
        body.Should().Be("My boy");
    }
}
