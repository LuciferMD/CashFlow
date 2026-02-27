namespace Auth.Models
{
    public class JwtOptions
    {
        public int ExpiersHours { get; set; }
        public string SecretKey { get; set; }
    }
}
