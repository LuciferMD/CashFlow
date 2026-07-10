using FluentAssertions;
using Auth.Services;

namespace AuthTests.Unit;

public sealed class PasswordHasherTests
{
    [Fact]
    public void Generate_ReturnsNonEmptyHash()
    {
        var hash = PasswordHasher.Generate("my-password");
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Generate_ReturnsDifferentHashForSamePassword()
    {
        // BCrypt salts every hash, so two calls with the same input must differ
        var hash1 = PasswordHasher.Generate("same-password");
        var hash2 = PasswordHasher.Generate("same-password");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Generate_HashStartsWithBcryptPrefix()
    {
        var hash = PasswordHasher.Generate("any-password");

        // BCrypt.Net EnhancedHashPassword outputs standard BCrypt format ($2a$ or $2b$)
        hash.Should().MatchRegex(@"^\$2[ab]\$");
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        const string password = "correct-horse-battery-staple";
        var hash = PasswordHasher.Generate(password);

        var result = PasswordHasher.Verify(password, hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hash = PasswordHasher.Generate("original-password");

        var result = PasswordHasher.Verify("wrong-password", hash);

        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithEmptyPassword_ReturnsFalse()
    {
        var hash = PasswordHasher.Generate("non-empty-password");

        var result = PasswordHasher.Verify("", hash);

        result.Should().BeFalse();
    }
}
