using HomeInventoryManager.Dto;
using HomeInventoryManager.Data;

namespace HomeInventoryManager.Api.Services.UserServices.Interfaces
{
    public interface IAuthService
    {
        Task<User?> RegisterUserAsync(UserRegisterDto request);
        Task<TokenResponseDto?> LoginUserAsync(UserLoginDto request);
        Task<TokenResponseDto?> LogoutUserAsync(int authenticatedUserId, UserLogoutDto request);
        Task<TokenResponseDto?> RefreshTokensAsync(int authenticatedUserId, RefreshTokenRequestDto request);
    }
}
