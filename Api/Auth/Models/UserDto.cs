namespace Auth.Models
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; private set; } = null!;
        public string Email { get; private set; } = null!;
    }
}
