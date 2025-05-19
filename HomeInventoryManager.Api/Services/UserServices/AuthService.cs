using HomeInventoryManager.Data;
using HomeInventoryManager.Dto;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using HomeInventoryManager.Api.Services.UserServices.Interfaces;
using HomeInventoryManager.Api.Utilities;

namespace HomeInventoryManager.Api.Services.UserServices
{
    public class AuthService(AppDbContext _context, IConfiguration _configuration, ILogger<AuthService> _logger) : IAuthService
    {
        public async Task<ServiceResult<User>> RegisterUserAsync(UserRegisterDto userRegisterDto)
        {
            // Check if username or email already exists
            if (await _context.USERSET.AnyAsync(u => u.user_name == userRegisterDto.UserName || u.email == userRegisterDto.Email))
            {
                _logger.LogWarning("Registration failed: Duplicate username or email for {UserName}", userRegisterDto.UserName);
                return ServiceResult<User>.Fail(ErrorCode.ERR_INVALID_CREDENTIALS);
            }

            // Create a new user
            var user = new User
            {
                user_name = userRegisterDto.UserName,
                email = userRegisterDto.Email.ToLowerInvariant(),
                first_name = userRegisterDto.FirstName,
                last_name = userRegisterDto.LastName,
                role = "Basic", // Default role
                created_at = DateTime.UtcNow
            };

            //TODO verify name complies with rules I am not aware of right now
            //TODO verify password complies with rules I am not aware of right now
            //email validation
            try
            {
                var mail = new MailAddress(userRegisterDto.Email);
            }
            catch (FormatException)
            {
                return ServiceResult<User>.Fail(ErrorCode.ERR_FORMAT_INVALID);
            }

            user.password_salt = PasswordUtility.GenerateSalt();
            var hashedPassword = PasswordUtility.HashPassword(userRegisterDto.PasswordString, user.password_salt);
            user.password_hash = hashedPassword;

            // Add user to database and save changes
            try
            {
                _context.USERSET.Add(user);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for {UserName}", userRegisterDto.UserName);
                return ServiceResult<User>.Fail(ErrorCode.ERR_INVALID_CREDENTIALS);
            }
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registration attempt successful: {UserName}", userRegisterDto.UserName);
            return ServiceResult<User>.Ok(user); ;
        }

        public async Task<ServiceResult<TokenResponseDto>> LoginUserAsync(UserLoginDto userLoginDto)
        {
            _logger.LogInformation("User login attempt: {UserName}", userLoginDto.UserName);

            int maxAttempts = _configuration.GetValue<int>("AppSettings:MaxFailedLoginAttempts");
            int lockoutMinutes = _configuration.GetValue<int>("AppSettings:LockoutTime");

            // Find the user by username or email
            User? user = await _context.USERSET.FirstOrDefaultAsync(u => u.user_name == userLoginDto.UserName || u.email == userLoginDto.UserName); //log in with either
            if (user == null)
            {
                _logger.LogWarning("Login failed: Invalid credentials for {UserName}", userLoginDto.UserName);
                return ServiceResult<TokenResponseDto>.Fail(ErrorCode.ERR_INVALID_CREDENTIALS);
            }

            if (user.lockout_until_time > DateTime.UtcNow) //check if user is locked out
            {
                _logger.LogWarning("Login failed: User {UserName} is locked out until {LockoutUntil}", userLoginDto.UserName, user.lockout_until_time);
                return ServiceResult<TokenResponseDto>.Fail(ErrorCode.ERR_ACCOUNT_LOCKED); ;
            }
            else if (user.failed_login_attempts >= maxAttempts)
            {
                user.failed_login_attempts = 0;
                _context.USERSET.Update(user);
                await _context.SaveChangesAsync();
            }

                var hashedPassword = PasswordUtility.HashPassword(userLoginDto.PasswordString, user.password_salt);

            if (!CryptographicOperations.FixedTimeEquals(hashedPassword, user.password_hash)) //neat. takes the same time to compare both strings to mitigate weak passwords
            {
                user.failed_login_attempts++;
                user.last_login_attempt_time = DateTime.UtcNow;

                if (user.failed_login_attempts >= maxAttempts)
                {
                    user.lockout_until_time = DateTime.UtcNow.AddMinutes(lockoutMinutes);
                    user.total_lockouts++;
                    _logger.LogWarning("Login failed: User {UserName} is now locked out until {LockoutUntil}", userLoginDto.UserName, user.lockout_until_time);
                }
                else
                {
                    _logger.LogWarning("Login failed: Invalid credentials for {UserName}", userLoginDto.UserName);
                }

                _context.USERSET.Update(user);
                await _context.SaveChangesAsync();
                return ServiceResult<TokenResponseDto>.Fail(ErrorCode.ERR_INVALID_CREDENTIALS);
            }

            // Update login security details
            user.last_login_time = DateTime.UtcNow;
            user.last_login_attempt_time = DateTime.UtcNow;
            user.failed_login_attempts = 0;

            _context.USERSET.Update(user);
            await _context.SaveChangesAsync();

            var tokenResponse = await TokenUtility.CreateTokenResponse(user, _configuration, _context);
            return ServiceResult<TokenResponseDto>.Ok(tokenResponse);
        }

        
        public async Task<ServiceResult<TokenResponseDto>> LogoutUserAsync(int authenticatedUserId, UserLogoutDto userLogoutDto)
        {
            if (authenticatedUserId != userLogoutDto.UserId)
            {
                return ServiceResult<TokenResponseDto>.Fail(ErrorCode.ERR_UNAUTHORIZED);
            }

            var user = await _context.USERSET.FindAsync(userLogoutDto.UserId);
            if (user == null)
            {
                return ServiceResult<TokenResponseDto>.Fail(ErrorCode.ERR_USER_NOT_FOUND);
            }
            
            user.refresh_token = null;
            user.refresh_token_time = null;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User logout: {UserId}", userLogoutDto.UserId);

            var tokenResponse = new TokenResponseDto { AccessToken = "", RefreshToken = "" };
            return ServiceResult<TokenResponseDto>.Ok(tokenResponse);
        }

        public async Task<ServiceResult<TokenResponseDto>> RefreshTokensAsync(int authenticatedUserId, RefreshTokenRequestDto refreshTokenRequestDto)
        {
            if (authenticatedUserId != refreshTokenRequestDto.UserId)
            {
                return ServiceResult<TokenResponseDto>.Fail(ErrorCode.ERR_UNAUTHORIZED);
            }

            var user = await TokenUtility.ValidateRefreshTokenAsync(refreshTokenRequestDto.UserId, refreshTokenRequestDto.RefreshToken, _context, _logger);
            if (user == null)
            {
                return ServiceResult<TokenResponseDto>.Fail(ErrorCode.ERR_USER_NOT_FOUND);
            }
            var tokenResponse = await TokenUtility.CreateTokenResponse(user, _configuration, _context);
            return ServiceResult<TokenResponseDto>.Ok(tokenResponse);
        }
    }
}
