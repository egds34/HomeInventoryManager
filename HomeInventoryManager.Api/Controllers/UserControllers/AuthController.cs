using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HomeInventoryManager.Dto;
using HomeInventoryManager.Data;
using Microsoft.AspNetCore.Authorization;
using HomeInventoryManager.Api.Services.UserServices.Interfaces;
using System.Security.Claims;

namespace HomeInventoryManager.Api.Controllers.UserEndpoints
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<TokenResponseDto>> Register(UserRegisterDto userRegisterDto)
        {
            var result = await authService.RegisterUserAsync(userRegisterDto);
            if (result is null)
            {
                return BadRequest("User already exists.");
            }
            return Ok(result);
        }

        //POST: Login user
        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login(UserLoginDto userLoginDto)
        {
            var result = await authService.LoginUserAsync(userLoginDto);
            if (result is null)
            {
                return BadRequest("Invalid username or password.");
            }
            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<TokenResponseDto>> Logout(UserLogoutDto userLogoutDto)
        {
            var authenticatedUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await authService.LogoutUserAsync(authenticatedUserId, userLogoutDto);

            if (result == null)
            {
                return BadRequest("User not found.");
            }
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto refreshTokenRequstDto)
        {
            var authenticatedUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await authService.RefreshTokensAsync(authenticatedUserId, refreshTokenRequstDto);

            if (result == null || result.AccessToken == null || result.RefreshToken == null)
            {
                return Unauthorized("Invalid refresh token.");
            }
            return Ok(result);
        }

        //This is to test authentication for testing.
        [Authorize]
        [HttpGet("check-authentication")]
        public IActionResult AuthenticationOnlyEndpoint()
        {
            return Ok("Authenticated.");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-auth-check")]
        public IActionResult AuthorizationAdminOnlyEndpoint()
        {
            return Ok("Authorized.");
        }

        [Authorize(Roles = "Basic")]
        [HttpGet("basic-auth-check")]
        public IActionResult AuthorizationBasicOnlyEndpoint()
        {
            return Ok("Authorizd.");
        }


    }
}
