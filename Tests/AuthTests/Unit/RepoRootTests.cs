using FluentAssertions;
using Auth.Infrastructure;

namespace AuthTests.Unit;

public sealed class RepoRootTests
{
    [Fact]
    public void Find_ReturnsNonNullString()
    {
        var result = RepoRoot.Find();
        result.Should().NotBeNull();
    }

    [Fact]
    public void Find_ReturnsNonEmptyString()
    {
        var result = RepoRoot.Find();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void Find_ReturnedPathIsAnExistingDirectory()
    {
        var result = RepoRoot.Find();
        Directory.Exists(result).Should().BeTrue(
            "Find() must return the repo root or current directory, both of which must exist on disk");
    }
}
