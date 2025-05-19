using HomeInventoryManager.Data;
using HomeInventoryManager.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HomeInventoryManager.Api.Utilities
{
    public static class TokenUtility
    {
        public static async Task<TokenResponseDto> CreateTokenResponse(
            User user,
            IConfiguration configuration,
            AppDbContext context)
        {
            return new TokenResponseDto
            {
                AccessToken = CreateToken(user, configuration),
                RefreshToken = await GenerateAndSaveRefreshToken(user, configuration, context)
            };
        }

        public static async Task<TokenResponseDto?> RefreshTokensAsync(
            RefreshTokenRequestDto refreshTokenRequestDto,
            AppDbContext context,
            IConfiguration configuration,
            ILogger logger)
        {
            var user = await ValidateRefreshTokenAsync(refreshTokenRequestDto.UserId, refreshTokenRequestDto.RefreshToken, context, logger);
            if (user == null)
            {
                return null;
            }
            return await CreateTokenResponse(user, configuration, context);
        }

        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public static async Task<string> GenerateAndSaveRefreshToken(
            User user,
            IConfiguration configuration,
            AppDbContext context)
        {
            var refreshToken = GenerateRefreshToken();
            user.refresh_token = refreshToken;
            user.refresh_token_time = DateTime.UtcNow.AddDays(configuration.GetValue<int>("AppSettings:RefreshTimeInDays"));
            await context.SaveChangesAsync();
            return refreshToken;
        }

        public static async Task<User?> ValidateRefreshTokenAsync(
            int userId,
            string refreshToken,
            AppDbContext context,
            ILogger logger)
        {
            var user = await context.USERSET.FindAsync(userId);
            if (user == null || user.refresh_token != refreshToken || user.refresh_token_time <= DateTime.UtcNow)
            {
                logger.LogWarning("Refresh token invalid or expired for user {UserId}", userId);
                return null;
            }
            return user;
        }

        // JWT Token creation
        public static string CreateToken(User user, IConfiguration configuration)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.user_id.ToString()),
                new Claim(ClaimTypes.Name, user.user_name),
                new Claim(ClaimTypes.Role, user.role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
