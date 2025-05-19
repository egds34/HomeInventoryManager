using HomeInventoryManager.Api.Utilities;
using HomeInventoryManager.Data;
using HomeInventoryManager.Dto;

namespace HomeInventoryManager.Api.Services.UserServices.Interfaces
{
    public interface IUserDeleteService
    {
        Task<ServiceResult<User>> DeleteUserPersonalAsync(int authenticatedUserId, UserLogoutDto request);
        Task<ServiceResult<User>> DeleteUserAsync(UserLogoutDto request);
    }
}
