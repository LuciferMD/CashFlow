namespace Auth.Models
{
    public class UserDto
    {
        private UserDto(Guid id, string name, string email, string passwordHash)
        {
            Id = id;
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
        }
        public Guid Id{ get; set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public static UserDto Create (Guid id, string name, string email, string passwordHash)
        {
            return new UserDto (id, name, email, passwordHash);
        }

    }
}
