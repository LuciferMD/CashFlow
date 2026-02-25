using Auth.Models;
using Auth.Repositories.Models;

namespace Auth.Interfaces
{
    public interface IUserService
    {
        public Task Register(UserRequestDto userRequest);
    }
}
