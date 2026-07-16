using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Auth.Controllers;
using Auth.Interfaces;
using Auth.Models;

namespace AuthTests.Unit;

public sealed class AuthControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly DefaultHttpContext _httpContext;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _httpContext = new DefaultHttpContext();
        _controller = new AuthController(NullLogger<AuthController>.Instance, _mockUserService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = _httpContext }
        };
    }

    private static UserRequestDto SomeRegisterRequest() =>
        new() { UserName = "alice", Email = "alice@example.com", Password = "secret" };

    private static LoginUserRequestDto SomeLoginRequest() =>
        new() { Email = "alice@example.com", Password = "secret" };

    // ── Register ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ReturnsOkResult()
    {
        _mockUserService.Setup(s => s.Register(It.IsAny<UserRequestDto>())).ReturnsAsync("some.jwt");

        var result = await _controller.Register(SomeRegisterRequest());

        // IStatusCodeHttpResult avoids needing a full DI container to execute the result
        result.Should().BeAssignableTo<IStatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Register_SetsCookieNamedGuardPass()
    {
        _mockUserService.Setup(s => s.Register(It.IsAny<UserRequestDto>())).ReturnsAsync("some.jwt");

        await _controller.Register(SomeRegisterRequest());

        var setCookie = _httpContext.Response.Headers["Set-Cookie"].ToString();
        setCookie.Should().Contain("GuardPass");
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WhenServiceReturnsEmptyString_ReturnsUnauthorized()
    {
        _mockUserService.Setup(s => s.Login(It.IsAny<LoginUserRequestDto>())).ReturnsAsync(string.Empty);

        var result = await _controller.Login(SomeLoginRequest());

        result.Should().BeAssignableTo<IStatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Login_WhenServiceReturnsToken_ReturnsOkResult()
    {
        _mockUserService.Setup(s => s.Login(It.IsAny<LoginUserRequestDto>())).ReturnsAsync("valid.jwt");

        var result = await _controller.Login(SomeLoginRequest());

        result.Should().BeAssignableTo<IStatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Login_WhenServiceReturnsToken_SetsCookieNamedGuardPass()
    {
        _mockUserService.Setup(s => s.Login(It.IsAny<LoginUserRequestDto>())).ReturnsAsync("valid.jwt");

        await _controller.Login(SomeLoginRequest());

        var setCookie = _httpContext.Response.Headers["Set-Cookie"].ToString();
        setCookie.Should().Contain("GuardPass");
    }
}
