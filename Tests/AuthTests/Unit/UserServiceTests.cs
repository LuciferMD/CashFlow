using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Auth.Interfaces;
using Auth.Models;
using Auth.Repositories.Models;
using Auth.Services;

namespace AuthTests.Unit;

public sealed class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockRepo;
    private readonly Mock<IJwtProvider> _mockJwt;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _mockRepo = new Mock<IUserRepository>();
        _mockJwt = new Mock<IJwtProvider>();
        _sut = new UserService(_mockRepo.Object, _mockJwt.Object, NullLogger<UserService>.Instance);
    }

    // ── Register ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_CallsRepositoryAdd()
    {
        _mockRepo.Setup(r => r.Add(It.IsAny<User>())).Returns(Task.CompletedTask);
        _mockJwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("token");

        await _sut.Register(new UserRequestDto { UserName = "u", Email = "u@test.com", Password = "pass" });

        _mockRepo.Verify(r => r.Add(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Register_DoesNotSavePasswordInPlainText()
    {
        const string plainPassword = "super-secret";
        User? capturedUser = null;

        _mockRepo.Setup(r => r.Add(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .Returns(Task.CompletedTask);
        _mockJwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("token");

        await _sut.Register(new UserRequestDto { UserName = "u", Email = "u@test.com", Password = plainPassword });

        capturedUser!.PasswordHash.Should().NotBe(plainPassword,
            "password must be hashed before persisting");
    }

    [Fact]
    public async Task Register_ReturnsTokenFromJwtProvider()
    {
        const string expectedToken = "expected.jwt.token";
        _mockRepo.Setup(r => r.Add(It.IsAny<User>())).Returns(Task.CompletedTask);
        _mockJwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns(expectedToken);

        var result = await _sut.Register(new UserRequestDto { UserName = "u", Email = "u@test.com", Password = "pass" });

        result.Should().Be(expectedToken);
    }

    [Fact]
    public async Task Register_WhenRepositoryThrows_PropagatesException()
    {
        _mockRepo.Setup(r => r.Add(It.IsAny<User>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var act = () => _sut.Register(new UserRequestDto { UserName = "u", Email = "u@test.com", Password = "pass" });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("DB error");
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WhenUserNotFound_ReturnsEmptyString()
    {
        _mockRepo.Setup(r => r.GetByEmail(It.IsAny<string>())).ReturnsAsync((User)null!);

        var result = await _sut.Login(new LoginUserRequestDto { Email = "ghost@test.com", Password = "pass" });

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Login_WhenPasswordIsIncorrect_ReturnsEmptyString()
    {
        var user = User.Create(Guid.NewGuid(), "u", "u@test.com", PasswordHasher.Generate("real-password"));
        _mockRepo.Setup(r => r.GetByEmail("u@test.com")).ReturnsAsync(user);

        var result = await _sut.Login(new LoginUserRequestDto { Email = "u@test.com", Password = "wrong-password" });

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ReturnsToken()
    {
        const string token = "valid.jwt.token";
        const string plainPassword = "correct-password";
        var user = User.Create(Guid.NewGuid(), "u", "u@test.com", PasswordHasher.Generate(plainPassword));
        _mockRepo.Setup(r => r.GetByEmail("u@test.com")).ReturnsAsync(user);
        _mockJwt.Setup(j => j.GenerateToken(user)).Returns(token);

        var result = await _sut.Login(new LoginUserRequestDto { Email = "u@test.com", Password = plainPassword });

        result.Should().Be(token);
    }

    [Fact]
    public async Task Login_WhenUserNotFound_DoesNotCallJwtProvider()
    {
        _mockRepo.Setup(r => r.GetByEmail(It.IsAny<string>())).ReturnsAsync((User)null!);

        await _sut.Login(new LoginUserRequestDto { Email = "nobody@test.com", Password = "pass" });

        _mockJwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Login_WhenPasswordIsWrong_DoesNotCallJwtProvider()
    {
        var user = User.Create(Guid.NewGuid(), "u", "u@test.com", PasswordHasher.Generate("correct"));
        _mockRepo.Setup(r => r.GetByEmail("u@test.com")).ReturnsAsync(user);

        await _sut.Login(new LoginUserRequestDto { Email = "u@test.com", Password = "wrong" });

        _mockJwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }
}
