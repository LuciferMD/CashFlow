using Auth.Interfaces;
using Auth.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
        public async Task Register(UserRequestDto userRequestDto)
        {
            await _userService.Register(userRequestDto);
        }
        [HttpPost("login")]
        public async Task<IResult> Login(LoginUserRequestDto loginRequest)
        {
            var token = await _userService.Login(loginRequest);
            
            return Results.Ok(token);
        }
    }
}
