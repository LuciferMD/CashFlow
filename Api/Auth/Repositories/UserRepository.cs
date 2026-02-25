using Auth.Interfaces;
using Auth.Models;
using Auth.Repositories.Context;
using Auth.Repositories.Models;

namespace Auth.Repositories
{
    public class UserRepository : IUserRepository
    {
        private AuthContext _context;
        public UserRepository(AuthContext context)
        {
            _context = context;
        }
        public async Task Add(User user)
        {
           await _context.AddAsync(user);
           await _context.SaveChangesAsync();
        }
    }
}
