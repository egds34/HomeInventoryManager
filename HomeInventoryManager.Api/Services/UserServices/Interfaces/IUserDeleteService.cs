using HomeInventoryManager.Data;
using HomeInventoryManager.Dto;

namespace HomeInventoryManager.Api.Services.UserServices.Interfaces
{
    public interface IUserDeleteService
    {
        Task<User?> DeleteUserPersonalAsync(int authenticatedUserId, UserLogoutDto request);
        Task<User?> DeleteUserAsync(UserLogoutDto request);
    }
}
