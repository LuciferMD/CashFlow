using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Auth.Infrastructure;

public static class RsaKeyLoader
{
    public static RsaSecurityKey LoadPrivateKey(string keyPath)
    {
        var pem = File.ReadAllText(keyPath);
        var rsa = RSA.Create();
        rsa.ImportFromPem(pem);
        return new RsaSecurityKey(rsa);
    }

    public static RsaSecurityKey LoadPublicKey(string keyPath)
    {
        var pem = File.ReadAllText(keyPath);
        var rsa = RSA.Create();
        rsa.ImportFromPem(pem);
        return new RsaSecurityKey(rsa);
    }

    public static string ResolveKeyPath(string repoRoot, string keyPath)
    {
        if (Path.IsPathRooted(keyPath))
            return keyPath;

        return Path.GetFullPath(Path.Combine(repoRoot, keyPath));
    }
}
