using HomeInventoryManager.Data;
using HomeInventoryManager.Dto;
using System.Net.Mail;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace HomeInventoryManager.Api.Services
{
    public class AuthService(AppDbContext _context, 
        IConfiguration _configuration, 
        ILogger<AuthService> _logger) : IAuthService
    {

        public async Task<User?> RegisterAsync(UserRegisterDto userRegisterDto)
        {
            // Check if username or email already exists
            if (await _context.USERSET.AnyAsync(u => u.user_name == userRegisterDto.UserName || u.email == userRegisterDto.Email))
            {
                _logger.LogWarning("Registration failed: Duplicate username or email for {UserName}", userRegisterDto.UserName);
                return null;
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
                return null;
            }

            // Generate a random salt
            using (var rng = RandomNumberGenerator.Create())
            {
                var salt = new byte[16];
                rng.GetBytes(salt);
                user.password_salt = salt;
            }

            //hash password with salt
            var hashedPassword = HashPassword(userRegisterDto.PasswordString, user.password_salt);
            user.password_hash = hashedPassword;

            // Add user to database and save changes
            try
            {
                _context.USERSET.Add(user);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for {UserName}", userRegisterDto.UserName);
                return null;
            }
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registration attempt successful: {UserName}", userRegisterDto.UserName);
            return user;
        }

        public async Task<TokenResponseDto?> LoginAsync(UserLoginDto userLoginDto)
        {
            _logger.LogInformation("User login attempt: {UserName}", userLoginDto.UserName);

            int maxAttempts = _configuration.GetValue<int>("AppSettings:MaxFailedLoginAttempts");
            int lockoutMinutes = _configuration.GetValue<int>("AppSettings:LockoutTime");

            // Find the user by username or email
            User? user = await _context.USERSET.FirstOrDefaultAsync(u => u.user_name == userLoginDto.UserName || u.email == userLoginDto.UserName); //log in with either
            if (user == null)
            {
                _logger.LogWarning("Login failed: Invalid credentials for {UserName}", userLoginDto.UserName);
                return null;
            }

            if (user.lockout_until_time > DateTime.UtcNow) //check if user is locked out
            {
                _logger.LogWarning("Login failed: User {UserName} is locked out until {LockoutUntil}", userLoginDto.UserName, user.lockout_until_time);
                return null;
            }
            else if (user.failed_login_attempts >= maxAttempts)
            {
                user.failed_login_attempts = 0;
                _context.USERSET.Update(user);
                await _context.SaveChangesAsync();
            }

                var hashedPassword = HashPassword(userLoginDto.PasswordString, user.password_salt);

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
                return null;
            }

            // Update login security details
            user.last_login_time = DateTime.UtcNow;
            user.last_login_attempt_time = DateTime.UtcNow;
            user.failed_login_attempts = 0;

            _context.USERSET.Update(user);
            await _context.SaveChangesAsync();

            return await CreateTokenResponse(user);
        }

        public async Task<TokenResponseDto?> LogoutAsync(UserLogoutDto userLogoutDto)
        {
            // Find the user by ID
            var user = await _context.USERSET.FindAsync(userLogoutDto.UserId);
            if (user == null)
            {
                return null;
            }
            
            user.refresh_token = null;
            user.refresh_token_time = null;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User logout: {UserId}", userLogoutDto.UserId);

            return new TokenResponseDto { AccessToken = "", RefreshToken = "" };
        }

        private async Task<TokenResponseDto> CreateTokenResponse(User user)
        {
            return new TokenResponseDto
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshToken(user)
            };
        }

        public async Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto refreshTokenRequestDto)
        {
            var user = await ValidateRefreshTokenAsync(refreshTokenRequestDto.UserId, refreshTokenRequestDto.RefreshToken);
            if (user == null)
            {
                return null;
            }
            return await CreateTokenResponse(user);
        }

        private byte[] HashPassword(string password, byte[] salt)
        {
            using (var hmac = new HMACSHA512(salt))
            {
                return hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshToken(User user)
        {
            var refreshToken = GenerateRefreshToken();
            user.refresh_token = refreshToken;
            user.refresh_token_time = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("AppSettings:RefreshTimeInDays"));
            await _context.SaveChangesAsync();
            return refreshToken;
        }

        private async Task<User?> ValidateRefreshTokenAsync(int userId, string refreshToken)
        {
            var user = await _context.USERSET.FindAsync(userId);
            if (user == null || user.refresh_token != refreshToken || user.refresh_token_time <= DateTime.UtcNow)
            {

                _logger.LogWarning("Refresh token invalid or expired for user {UserId}", userId);
                return null;
            }
            return user;
        }

        //JWT Token creation
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.user_id.ToString()),
                new Claim(ClaimTypes.Name, user.user_name),
                new Claim(ClaimTypes.Role, user.role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSettings:Token")!)); // Use a secure key
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("AppSettings:Issuer"),
                audience: _configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.Now.AddDays(1),//TODO make to minutes I think
                signingCredentials: creds
            );

            // Token creation logic goes here
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
