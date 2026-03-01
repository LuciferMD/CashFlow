using System.ComponentModel.DataAnnotations;

namespace Auth.Models
{
    public class LoginUserRequestDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
