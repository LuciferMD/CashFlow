using Auth.Models;
using Auth.Repositories.Models;

namespace Auth.Interfaces
{
    public interface IUserRepository
    {
        public Task Add(User user);
        public Task<User?> GetByEmail(string email);
        public Task<bool> EmailExists(string email);
    }
}
