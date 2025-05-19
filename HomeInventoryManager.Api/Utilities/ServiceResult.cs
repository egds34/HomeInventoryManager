using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace HomeInventoryManager.Api.Utilities
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public ErrorCode? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }

        public static ServiceResult<T> Ok(T data) => new() { Success = true, Data = data };
        public static ServiceResult<T> Fail(ErrorCode errorCode, string? customMessage = null) => new()
        {
            Success = false,
            ErrorCode = errorCode,
            ErrorMessage = ErrorCodeList.GetErrorMessage(errorCode) + (customMessage != null ? $": {customMessage}" : "")
        };


    }
}
