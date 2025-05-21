namespace HomeInventoryManager.Api.Utilities
{
    public static class ErrorCodeList
    {
        public static string GetErrorMessage(ErrorCode errorCode)
        {
            return errorCode switch
            {
                // Auth (1000s)
                ErrorCode.ERR_INVALID_CREDENTIALS => "Username or password is incorrect.",
                ErrorCode.ERR_ACCOUNT_LOCKED => "Too many failed login attempts; account is temporarily locked.",
                ErrorCode.ERR_UNAUTHORIZED => "User is not authorized for the requested action.",
                ErrorCode.ERR_FORBIDDEN_OPERATION => "Authenticated but not allowed to perform this action.",
                ErrorCode.ERR_TOKEN_EXPIRED => "The authentication token is no longer valid.",
                ErrorCode.ERR_TOKEN_INVALID => "Provided token is malformed or unrecognized.",
                ErrorCode.ERR_USER_NOT_FOUND => "User was not found in database.",

                // Validation of input (2000s)
                ErrorCode.ERR_VALIDATION_FAILED => "Input fields failed validation.",
                ErrorCode.ERR_REQUIRED_FIELD => "A required field is missing.",
                ErrorCode.ERR_FORMAT_INVALID => "Field value does not match expected format.",
                ErrorCode.ERR_DUPLICATE_ENTRY => "An entry with the same unique field already exists.",
                ErrorCode.ERR_STRING_TOO_LONG => "String exceeds max allowed length.",
                ErrorCode.ERR_STRING_TOO_SHORT => "String does not meet minimum length requirement.",

                // Database (3000s)
                ErrorCode.ERR_DB_CONNECTION => "Unable to connect to database.",
                ErrorCode.ERR_DB_CONSTRAINT_FAIL => "Foreign key or other constraint failed.",
                ErrorCode.ERR_DB_TIMEOUT => "Database operation timed out.",
                ErrorCode.ERR_RECORD_NOT_FOUND => "The requested record does not exist.",
                ErrorCode.ERR_DUPLICATE_KEY => "Unique constraint violation.",
                ErrorCode.ERR_UNKNOWN => "Unsure what the issue is for now.",

                // Logic (4000s)
                ErrorCode.ERR_CONFLICTING_STATE => "Operation not allowed in current state.",
                ErrorCode.ERR_MAX_ATTEMPTS_EXCEEDED => "Exceeded the number of allowed attempts.",
                ErrorCode.ERR_DEPENDENCY_EXISTS => "Cannot delete/update due to existing references.",
                ErrorCode.ERR_RATE_LIMIT => "Too many requests in a short time.",

                // System/unknown (5000s)
                ErrorCode.ERR_INTERNAL => "Unhandled or unknown internal error.",
                ErrorCode.ERR_SERVICE_UNAVAILABLE => "Service temporarily down or restarting.",
                ErrorCode.ERR_NOT_IMPLEMENTED => "Feature not yet implemented.",
                ErrorCode.ERR_CONFIG_MISSING => "Application configuration error.",

                // Security (6000s)
                ErrorCode.ERR_HASH_MISMATCH => "Password hash comparison failed.",
                ErrorCode.ERR_SUSPICIOUS_ACTIVITY => "Unusual behavior detected.",
                ErrorCode.ERR_INVALID_SIGNATURE => "Token or data has invalid signature.",

                _ => "An unknown error occurred."
            };
        }
    }

    public enum ErrorCode
    {
        // Auth (1000s)
        ERR_INVALID_CREDENTIALS = 1001,
        ERR_ACCOUNT_LOCKED = 1002,
        ERR_UNAUTHORIZED = 1003,
        ERR_FORBIDDEN_OPERATION = 1004,
        ERR_TOKEN_EXPIRED = 1005,
        ERR_TOKEN_INVALID = 1006,
        ERR_USER_NOT_FOUND = 1007,


        // Validation of input (2000s)
        ERR_VALIDATION_FAILED = 2001,
        ERR_REQUIRED_FIELD = 2002,
        ERR_FORMAT_INVALID = 2003,
        ERR_DUPLICATE_ENTRY = 2004,
        ERR_STRING_TOO_LONG = 2005,
        ERR_STRING_TOO_SHORT = 2006,

        // Database (3000s)
        ERR_DB_CONNECTION = 3001,
        ERR_DB_CONSTRAINT_FAIL = 3002,
        ERR_DB_TIMEOUT = 3003,
        ERR_RECORD_NOT_FOUND = 3004,
        ERR_DUPLICATE_KEY = 3005,
        ERR_UNKNOWN = 3006,

        // Logic (4000s)
        ERR_CONFLICTING_STATE = 4001,
        ERR_MAX_ATTEMPTS_EXCEEDED = 4002,
        ERR_DEPENDENCY_EXISTS = 4003,
        ERR_RATE_LIMIT = 4004,

        // System/unknown (5000s)
        ERR_INTERNAL = 5001,
        ERR_SERVICE_UNAVAILABLE = 5002,
        ERR_NOT_IMPLEMENTED = 5003,
        ERR_CONFIG_MISSING = 5004,

        // Security (6000s)
        ERR_HASH_MISMATCH = 6001,
        ERR_SUSPICIOUS_ACTIVITY = 6002,
        ERR_INVALID_SIGNATURE = 6003
    }

}
