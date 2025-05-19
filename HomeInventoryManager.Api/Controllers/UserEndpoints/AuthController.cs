using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HomeInventoryManager.Dto;
using HomeInventoryManager.Data;
using Microsoft.AspNetCore.Identity;
using System.Text;
using System.Security.Cryptography;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using HomeInventoryManager.Api.Services;
using Microsoft.AspNetCore.Authorization;

namespace HomeInventoryManager.Api.Controllers.UserEndpoints
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {       
        //POST: Register a new user
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserRegisterDto userRegisterDto)
        {
            var user = await authService.RegisterAsync(userRegisterDto);
            if (user == null)
            {
                return BadRequest("Username or email already exists.");
            }
            return Ok(user);
        }

        // Updated code for the Login method
        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login(UserLoginDto userLoginDto)
        {
            var result = await authService.LoginAsync(userLoginDto);
            if (result is null)
            {
                return BadRequest("Invalid username or password.");
            }
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto refreshTokenRequstDto)
        {
            var result = await authService.RefreshTokensAsync(refreshTokenRequstDto);
            if (result == null || result.AccessToken == null || result.RefreshToken == null)
            {
                return Unauthorized("Invalid refresh token.");
            }
            return Ok(result);
        }

        //This is to test authentication for testing.
        [Authorize]
        [HttpGet]
        public IActionResult AuthenticationOnlyEndpoint()
        {
            return Ok("Authenticated.");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("AdminAsses")]
        public IActionResult AuthorizationAdminOnlyEndpoint()
        {
            return Ok("Authorized.");
        }

        [Authorize(Roles = "Basic")]
        [HttpGet("BasicBitches")]
        public IActionResult AuthorizationBasicOnlyEndpoint()
        {
            return Ok("Authorizd.");
        }


    }
}
