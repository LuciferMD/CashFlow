namespace Auth.Models
{
    public class JwtOptions
    {
        public int ExpiersHours { get; set; }
        public string PrivateKeyPath { get; set; } = string.Empty;
        public string PublicKeyPath { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
    }
}
