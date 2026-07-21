using Auth.Models;

namespace Auth.Interfaces
{
    public interface IUserService
    {
        public Task<string> Register(UserRequestDto userRequest);
        public Task<string> Login(LoginUserRequestDto loginUserRequestDto);
    }
}
