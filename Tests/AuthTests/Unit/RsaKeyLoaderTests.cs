using FluentAssertions;
using Auth.Infrastructure;

namespace AuthTests.Unit;

public sealed class RsaKeyLoaderTests
{
    [Fact]
    public void ResolveKeyPath_WhenPathIsAbsolute_ReturnsPathUnchanged()
    {
        const string absolutePath = @"C:\keys\private.pem";

        var result = RsaKeyLoader.ResolveKeyPath(@"C:\repo", absolutePath);

        result.Should().Be(absolutePath);
    }

    [Fact]
    public void ResolveKeyPath_WhenPathIsRelative_CombinesWithRepoRoot()
    {
        const string repoRoot = @"C:\repo";
        const string relativePath = @"keys\private.pem";

        var result = RsaKeyLoader.ResolveKeyPath(repoRoot, relativePath);

        result.Should().Contain("repo").And.Contain("keys").And.Contain("private.pem");
    }

    [Fact]
    public void LoadPrivateKey_WhenFileDoesNotExist_Throws()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pem");

        var act = () => RsaKeyLoader.LoadPrivateKey(nonExistentPath);

        act.Should().Throw<FileNotFoundException>();
    }
}
