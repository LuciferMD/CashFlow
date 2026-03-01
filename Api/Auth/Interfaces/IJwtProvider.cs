using Auth.Repositories.Models;

namespace Auth.Interfaces
{
    public interface IJwtProvider
    {
        public string GenerateToken(User user);
    }
}
