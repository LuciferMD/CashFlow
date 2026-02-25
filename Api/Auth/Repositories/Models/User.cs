using Auth.Models;

namespace Auth.Repositories.Models
{
    public class User
    {
        private User(Guid id, string name, string email, string passwordHash)
        {
            Id = id;
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
        }
        public Guid Id { get; set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public static User Create(Guid id, string name, string email, string passwordHash)
        {
            return new User(id, name, email, passwordHash);
        }
    }
}
