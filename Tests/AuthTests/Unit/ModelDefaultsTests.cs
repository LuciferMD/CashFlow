using FluentAssertions;
using Auth.Models;

namespace AuthTests.Unit;

public sealed class ModelDefaultsTests
{
    [Fact]
    public void JwtOptions_DefaultPrivateKeyPath_IsEmptyString()
    {
        var options = new JwtOptions();
        options.PrivateKeyPath.Should().Be(string.Empty);
    }

    [Fact]
    public void JwtOptions_DefaultPublicKeyPath_IsEmptyString()
    {
        var options = new JwtOptions();
        options.PublicKeyPath.Should().Be(string.Empty);
    }
}
