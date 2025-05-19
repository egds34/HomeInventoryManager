using System.Text.RegularExpressions;
using System.Net.Mail;
using HomeInventoryManager.Data;
using HomeInventoryManager.Dto;
using System.ComponentModel.DataAnnotations;

namespace HomeInventoryManager.Api.Utilities
{
    public static class ValidationUtility
    {
        private const string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*])[A-Za-z\d!@#$%^&*]{8,16}$";
        private const string alphanumericUnderscorePattern = @"^[A-Za-z0-9_]+$";
        private const string alphanumericPattern = @"^[A-Za-z]+$";
        
        public static bool VerifyEmail(string email)
        {
            try
            {
                var mail = new MailAddress(email);
            }
            catch (FormatException)
            {
                return false;
            }
            return true;
        }

        private static bool VerifyUsername(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            if (str.Length < 1 || str.Length > 16)
            {
                return false;
            }

            // Check if the string matches the pattern
            return Regex.IsMatch(str, alphanumericUnderscorePattern);
        }

        private static bool VerifyName(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            if (str.Length < 1 || str.Length > 16)
            {
                return false;
            }

            // Check if the string matches the pattern
            return Regex.IsMatch(str, alphanumericPattern);
        }

        private static bool VerifyPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }
            if (password.Length < 8 || password.Length > 16)
            {
                return false;
            }
            // Check if the password matches the pattern
            return Regex.IsMatch(password, passwordPattern);
        }

        private static async Task<bool> VerifyWithTimeoutAsync(Func<string, bool> verifyFunc, string input, int timeoutMs = 2000)
        {
            using var cts = new CancellationTokenSource(timeoutMs);
            try
            {
                return await Task.Run(() => verifyFunc(input), cts.Token);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        public static Task<bool> VerifyUsernameWithTimeoutAsync(string username, int timeoutMs = 2000)
            => VerifyWithTimeoutAsync(VerifyUsername, username, timeoutMs);

        public static Task<bool> VerifyNameWithTimeoutAsync(string name, int timeoutMs = 2000)
            => VerifyWithTimeoutAsync(VerifyName, name, timeoutMs);

        public static Task<bool> VerifyPasswordWithTimeoutAsync(string password, int timeoutMs = 2000)
            => VerifyWithTimeoutAsync(VerifyPassword, password, timeoutMs);

        public static ValidationResultDto ValidateUserFieldFormat(UserRegisterDto dto)
        {
            var result = new ValidationResultDto();

            if (!VerifyEmail(dto.Email))
                result.Errors["Email"] = "Invalid email format.";

            if (!VerifyUsername(dto.UserName))
                result.Errors["UserName"] = "Username must be 1-16 characters, alphanumeric or underscore.";

            if (!VerifyName(dto.FirstName))
                result.Errors["FirstName"] = "First name must be 1-16 alphabetic characters.";

            if (!VerifyName(dto.LastName))
                result.Errors["LastName"] = "Last name must be 1-16 alphabetic characters.";

            if (!VerifyPassword(dto.PasswordString))
                result.Errors["Password"] = "Password does not meet complexity requirements.";

            return result;
        }
    }
}
