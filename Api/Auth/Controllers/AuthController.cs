using Auth.Interfaces;
using Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IUserService _userService;
        public AuthController(ILogger<AuthController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IResult> Register(UserRequestDto userRequestDto)
        {
            try
            {
                var token = await _userService.Register(userRequestDto);
                Response.Cookies.Append("GuardPass", token);
                return Results.Ok();
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IResult> Login(LoginUserRequestDto loginRequest)
        {
            _logger.LogInformation("Login request received for {Email}", loginRequest.Email);

            var token = await _userService.Login(loginRequest);
            if (token == string.Empty)
            {
                _logger.LogWarning("Login rejected for {Email}", loginRequest.Email);
                return Results.Unauthorized();
            }

            Response.Cookies.Append("GuardPass", token);

            return Results.Ok();
        }

        [Authorize]
        [HttpGet]
        public Task<string> Test()
        {
            return Task.FromResult("My boy");
        }
    }
}
