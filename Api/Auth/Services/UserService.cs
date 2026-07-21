using Auth.Interfaces;
using Auth.Models;
using Auth.Repositories.Models;

namespace Auth.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IJwtProvider jwtProvider,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _jwtProvider = jwtProvider;
            _logger = logger;
        }

        public async Task<string> Register(UserRequestDto userRequestDto)
        {
            if (await _userRepository.EmailExists(userRequestDto.Email))
            {
                throw new InvalidOperationException("Email already registered");
            }

            var passwordHash = PasswordHasher.Generate(userRequestDto.Password);
            var newUser = User.Create(Guid.NewGuid(), userRequestDto.UserName, userRequestDto.Email, passwordHash);

            await _userRepository.Add(newUser);
            return _jwtProvider.GenerateToken(newUser);

        }

        public async Task<string> Login(LoginUserRequestDto loginRequest)
        {
            var user = await _userRepository.GetByEmail(loginRequest.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed – user not found for email {Email}", loginRequest.Email);
                return string.Empty;
            }

            var result = PasswordHasher.Verify(loginRequest.Password, user.PasswordHash);
            if (result == false)
            {
                _logger.LogWarning("Login failed – invalid password for email {Email}", loginRequest.Email);
                return string.Empty;
            }

            var token = _jwtProvider.GenerateToken(user);

            _logger.LogInformation("User logged in: {UserName} ({Email})", user.Name, user.Email);

            return token;
        }
    }
}
