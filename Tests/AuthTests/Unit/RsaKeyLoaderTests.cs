using FluentAssertions;
using Auth.Infrastructure;

namespace AuthTests.Unit;

public sealed class RsaKeyLoaderTests
{
    [Fact]
    public void ResolveKeyPath_WhenPathIsAbsolute_ReturnsPathUnchanged()
    {
        var absolutePath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "keys", "private.pem"));

        var result = RsaKeyLoader.ResolveKeyPath(Path.GetTempPath(), absolutePath);

        result.Should().Be(absolutePath);
    }

    [Fact]
    public void ResolveKeyPath_WhenPathIsRelative_CombinesWithRepoRoot()
    {
        var repoRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "repo"));
        var relativePath = Path.Combine("keys", "private.pem");

        var result = RsaKeyLoader.ResolveKeyPath(repoRoot, relativePath);

        result.Should().Be(Path.GetFullPath(Path.Combine(repoRoot, relativePath)));
    }

    [Fact]
    public void LoadPrivateKey_WhenFileDoesNotExist_Throws()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pem");

        var act = () => RsaKeyLoader.LoadPrivateKey(nonExistentPath);

        act.Should().Throw<FileNotFoundException>();
    }
}
