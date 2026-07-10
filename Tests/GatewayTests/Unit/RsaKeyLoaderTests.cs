using FluentAssertions;
using Gateway.Infrastructure;

namespace GatewayTests.Unit;

public sealed class RsaKeyLoaderTests
{
    [Fact]
    public void ResolveKeyPath_WhenPathIsAbsolute_ReturnsPathUnchanged()
    {
        const string absolutePath = @"C:\keys\public.pem";

        var result = RsaKeyLoader.ResolveKeyPath(@"C:\repo", absolutePath);

        result.Should().Be(absolutePath);
    }

    [Fact]
    public void ResolveKeyPath_WhenPathIsRelative_CombinesWithRepoRoot()
    {
        const string repoRoot = @"C:\repo";
        const string relativePath = @"keys\public.pem";

        var result = RsaKeyLoader.ResolveKeyPath(repoRoot, relativePath);

        result.Should().Contain("repo").And.Contain("keys").And.Contain("public.pem");
    }

    [Fact]
    public void LoadPublicKey_WhenFileDoesNotExist_Throws()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pem");

        var act = () => RsaKeyLoader.LoadPublicKey(nonExistentPath);

        act.Should().Throw<FileNotFoundException>();
    }
}
