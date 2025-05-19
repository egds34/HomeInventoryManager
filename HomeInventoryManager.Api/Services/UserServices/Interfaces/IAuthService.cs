using HomeInventoryManager.Dto;
using HomeInventoryManager.Data;
using HomeInventoryManager.Api.Utilities;

namespace HomeInventoryManager.Api.Services.UserServices.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<User>> RegisterUserAsync(UserRegisterDto request);
        Task<ServiceResult<TokenResponseDto>> LoginUserAsync(UserLoginDto request);
        Task<ServiceResult<TokenResponseDto>> LogoutUserAsync(int authenticatedUserId, UserLogoutDto request);
        Task<ServiceResult<TokenResponseDto>> RefreshTokensAsync(int authenticatedUserId, RefreshTokenRequestDto request);
    }
}
