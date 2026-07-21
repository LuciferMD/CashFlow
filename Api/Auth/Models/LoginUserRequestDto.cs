namespace Auth.Models
{
    public class LoginUserRequestDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
