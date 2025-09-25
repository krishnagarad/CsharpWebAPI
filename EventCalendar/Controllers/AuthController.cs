using EventCalendar.Application.Contracts.Security;
using EventCalendar.Application.Contracts.Security.Models;
using EventCalendar.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventCalendar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;
        public AuthController(IAuthService _authService)
        {
            authService = _authService;
        }
        public static User user = new User();


        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            var user = await authService.RegisterAsync(request);
            if ((user == null))
            {
                return BadRequest("User already exists");
            }
            return Ok(user);
        }
        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login(UserDto request)
        {

            var token = await authService.LoginAsync(request);
            if (token is null)
            {
                return BadRequest("Username or password is incorrect");
            }
            return Ok(token);
        }
        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto request)
        {
            var token = await authService.RefreshTokensAsync(request);
            if (token is null || token.AccessToken is null || token.RefreshToken is null)
            {
                return BadRequest("Invalid client request");
            }
            return Ok(token);
        }

        [Authorize]
        [HttpGet("authenticated")]
        public IActionResult AuthenticatedUsersOnly()
        {
            return Ok("If you see this, you are authenticated");
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("authenticatedAdmin")]
        public IActionResult AuthenticatedAdminUsersOnly()
        {
            return Ok("If you see this, you are authenticated");
        }

    }
}
