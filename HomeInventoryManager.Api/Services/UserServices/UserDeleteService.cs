using HomeInventoryManager.Dto;
using HomeInventoryManager.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using HomeInventoryManager.Api.Services.UserServices.Interfaces;
using HomeInventoryManager.Api.Utilities;

namespace HomeInventoryManager.Api.Services.UserServices
{
    public class UserDeleteService(AppDbContext _context, ILogger<AuthService> _logger) : IUserDeleteService
    {
        public async Task<ServiceResult<User>> DeleteUserPersonalAsync(int authenticatedUserId, UserLogoutDto userLogoutDto)
        {
            if (authenticatedUserId != userLogoutDto.UserId)
            {
                return ServiceResult<User>.Fail(ErrorCode.ERR_UNAUTHORIZED);
            }

            var user = await _context.USERSET.FindAsync(userLogoutDto.UserId);
            if (user == null)
            {
                return ServiceResult<User>.Fail(ErrorCode.ERR_USER_NOT_FOUND);
            }

            _context.USERSET.Remove(user);
            await _context.SaveChangesAsync();
            return ServiceResult < User > .Ok(user);
        }

        public async Task<ServiceResult<User>> DeleteUserAsync(UserLogoutDto userLogoutDto)
        {
            var user = await _context.USERSET.FindAsync(userLogoutDto.UserId);
            if (user == null)
            {
                return ServiceResult<User>.Fail(ErrorCode.ERR_USER_NOT_FOUND);
            }

            _context.USERSET.Remove(user);
            await _context.SaveChangesAsync();
            return ServiceResult<User>.Ok(user);
        }
    }
}
