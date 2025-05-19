using HomeInventoryManager.Dto;
using HomeInventoryManager.Data;

namespace HomeInventoryManager.Api.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserRegisterDto request);
        Task<TokenResponseDto?> LoginAsync(UserLoginDto request);
        Task<TokenResponseDto?> LogoutAsync(UserLogoutDto request);
        Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request);
    }
}
