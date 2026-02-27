using Auth.Interfaces;
using Auth.Models;
using Auth.Repositories.Models;

namespace Auth.Services
{
    public class UserService : IUserService
    {
        private IUserRepository _userRepository;
        private IJwtProvider _jwtProvider;
        public UserService(IUserRepository userRepository, IJwtProvider jwtProvider)
        {
            _userRepository = userRepository;
            _jwtProvider = jwtProvider;
        }
        public async Task Register(UserRequestDto userRequestDto)
        {
            var passwordHash = PasswordHasher.Generate(userRequestDto.Password);
            var newUser = User.Create(Guid.NewGuid(), userRequestDto.UserName, userRequestDto.Email, passwordHash);
            await _userRepository.Add(newUser);
        }

        public async Task<string> Login(LoginUserRequestDto loginRequest)
        {
            var user = await _userRepository.GetByEmail(loginRequest.Email);
            if(user == null)
            {
                return string.Empty;
            }

            var result = PasswordHasher.Verify(loginRequest.Password, user.PasswordHash);
            if (result == false)
            {
                return string.Empty;
            }

            var token = _jwtProvider.GenerateToken(user);

            return token;
        }
    }
}
