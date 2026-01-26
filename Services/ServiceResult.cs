namespace NotatApp.Services
{
    public static class ServiceErrors
    {
        public const string None = "None";
        public const string Validation = "Validation";
        public const string NotFound = "NotFound";
        public const string Unauthorized = "Unauthorized";
        public const string Failure = "Failure";
    }

    public class ServiceResult
    {
        public bool Ok { get; }
        public string Error { get; }
        public string? Message { get; }

        private ServiceResult(bool ok, string error, string? message = null)
        {
            Ok = ok;
            Error = error;
            Message = message;
        }

        public static ServiceResult Success(string? message = null) =>
            new ServiceResult(true, ServiceErrors.None, message);

        public static ServiceResult NotFound(string message) =>
            new ServiceResult(false, ServiceErrors.NotFound, message);

        public static ServiceResult Validation(string message) =>
            new ServiceResult(false, ServiceErrors.Validation, message);

        public static ServiceResult Unauthorized(string message) =>
            new ServiceResult(false, ServiceErrors.Unauthorized, message);

        public static ServiceResult Fail(string message) =>
            new ServiceResult(false, ServiceErrors.Failure, message);
    }
}
