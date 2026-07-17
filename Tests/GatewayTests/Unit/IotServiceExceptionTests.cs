using FluentAssertions;
using Gateway.Services;

namespace GatewayTests.Unit;

public sealed class IotServiceExceptionTests
{
    [Fact]
    public void Constructor_SetsMessage()
    {
        var ex = new IotServiceException("upstream error");

        ex.Message.Should().Be("upstream error");
    }

    [Fact]
    public void Constructor_WithNoRetryAfter_RetryAfterIsNull()
    {
        var ex = new IotServiceException("rate limit");

        ex.RetryAfter.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithRetryAfter_RetryAfterIsStored()
    {
        var delay = TimeSpan.FromSeconds(30);

        var ex = new IotServiceException("rate limit", delay);

        ex.RetryAfter.Should().Be(delay);
    }
}
