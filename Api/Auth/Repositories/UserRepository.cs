using Auth.Interfaces;
using Auth.Models;
using Auth.Repositories.Context;
using Auth.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth.Repositories
{
    public class UserRepository : IUserRepository
    {
        private AuthDbContext _context;
        public UserRepository(AuthDbContext context)
        {
            _context = context;
        }
        public async Task Add(User user)
        {
           await _context.AddAsync(user);
           await _context.SaveChangesAsync();
        }

        public async Task<User> GetByEmail(string email)
        {
            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);

            return user;
        }
    }
}
