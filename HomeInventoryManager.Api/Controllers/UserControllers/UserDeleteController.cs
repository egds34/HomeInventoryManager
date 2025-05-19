using HomeInventoryManager.Data;
using HomeInventoryManager.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HomeInventoryManager.Api.Services.UserServices.Interfaces;
using System.Security.Claims;

namespace HomeInventoryManager.Api.Controllers.UserEndpoints
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserDeleteController(IUserDeleteService userDeleteService) : ControllerBase
    {
        [Authorize]
        [HttpPost("delete")]
        public async Task<ActionResult<User>> DeleteUser(UserLogoutDto userLogoutDto)
        {
            var authenticatedUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await userDeleteService.DeleteUserPersonalAsync(authenticatedUserId, userLogoutDto);
            if (!result.Success)
            {
                return BadRequest(new { result.ErrorCode, result.ErrorMessage });
            }
            return Ok(result.Data);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("superuser-delete")]
        public async Task<ActionResult<User>> DeleteUserAsAdmin(UserLogoutDto userLogoutDto)
        {
            var result = await userDeleteService.DeleteUserAsync( userLogoutDto);
            if (!result.Success)
            {
                return BadRequest(new { result.ErrorCode, result.ErrorMessage });
            }
            return Ok(result.Data);
        }
    }
}
