using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Gateway.Infrastructure;

public static class RsaKeyLoader
{
    public static RsaSecurityKey LoadPublicKey(string keyPath)
    {
        var pem = File.ReadAllText(keyPath);
        var rsa = RSA.Create();
        rsa.ImportFromPem(pem);
        return new RsaSecurityKey(rsa);
    }

    public static string ResolveKeyPath(string repoRoot, string keyPath)
    {
        if (System.IO.Path.IsPathRooted(keyPath))
            return keyPath;

        return System.IO.Path.GetFullPath(System.IO.Path.Combine(repoRoot, keyPath));
    }
}
