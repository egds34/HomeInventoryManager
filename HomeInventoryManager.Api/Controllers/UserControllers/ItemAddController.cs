using HomeInventoryManager.Dto;
using Microsoft.AspNetCore.Mvc;
using HomeInventoryManager.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HomeInventoryManager.Api.Controllers.UserControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemAddController(IItemAddService itemAddService) : ControllerBase
    {
        [Authorize]
        [HttpPost("item")]
        public async Task<IActionResult> AddItem(ItemAddDto item)
        {
            item.user_id = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var id = await itemAddService.AddItemAsync(item);
            return Ok(new { id.Data });
        }
    }
}
