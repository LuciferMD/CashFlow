using Auth.Interfaces;
using Auth.Models;
using Microsoft.AspNetCore.Authorization;
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
        public async Task<IResult> Register(UserRequestDto userRequestDto)
        {
            var token = await _userService.Register(userRequestDto);
            Response.Cookies.Append("GuardPass", token);

            return Results.Ok();
        }

        [HttpPost("login")]
        public async Task<IResult> Login(LoginUserRequestDto loginRequest)
        {
            var token = await _userService.Login(loginRequest);
            if(token == string.Empty)
            {
                return Results.Unauthorized();
            }
            else
            {
                Response.Cookies.Append("GuardPass", token);

                return Results.Ok();
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<string> Test()
        {
            return "My boy";
        }
    }
}
