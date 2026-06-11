namespace Gateway.Models;

public class JwtValidationOptions
{
    public string PublicKeyPath { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
