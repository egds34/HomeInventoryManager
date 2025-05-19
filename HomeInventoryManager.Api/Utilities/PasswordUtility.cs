using HomeInventoryManager.Data;
using System.Security.Cryptography;
using System.Text;

namespace HomeInventoryManager.Api.Utilities
{
    public static class PasswordUtility
    {
        public static byte[] HashPassword(string password, byte[] salt)
        {
            using (var hmac = new HMACSHA512(salt))
            {
                return hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public static bool VerifyPassword(string password, byte[] storedHash, byte[] salt)
        {
            var hashedPassword = HashPassword(password, salt);
            return CryptographicOperations.FixedTimeEquals(hashedPassword, storedHash);
        }

        public static byte[] GenerateSalt()
        {
            var salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
    }
}
