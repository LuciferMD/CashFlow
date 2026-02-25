using Auth.Interfaces;
using Auth.Models;
using Auth.Repositories.Models;

namespace Auth.Services
{
    public class UserService : IUserService
    {
        private IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task Register(UserRequestDto userRequestDto)
        {
            var passwordHash = PasswordHasher.Generate(userRequestDto.Password);
            var newUser = User.Create(Guid.NewGuid(), userRequestDto.UserName, userRequestDto.Email, passwordHash);
            await _userRepository.Add(newUser);

        }
    }
}
