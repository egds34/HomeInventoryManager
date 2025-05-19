using HomeInventoryManager.Dto;
using HomeInventoryManager.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using HomeInventoryManager.Api.Services.UserServices.Interfaces;

namespace HomeInventoryManager.Api.Services.UserServices
{
    public class UserDeleteService(AppDbContext _context, ILogger<AuthService> _logger) : IUserDeleteService
    {
        public async Task<User?> DeleteUserPersonalAsync(int authenticatedUserId, UserLogoutDto userLogoutDto)
        {
            if (authenticatedUserId != userLogoutDto.UserId)
            {
                return null;
            }

            var user = await _context.USERSET.FindAsync(userLogoutDto.UserId);
            if (user == null)
            {
                return null;
            }

            _context.USERSET.Remove(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> DeleteUserAsync(UserLogoutDto userLogoutDto)
        {
            var user = await _context.USERSET.FindAsync(userLogoutDto.UserId);
            if (user == null)
            {
                return null;
            }

            _context.USERSET.Remove(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
