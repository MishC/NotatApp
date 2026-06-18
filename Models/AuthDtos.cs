namespace NotatApp.Models
{
    // DTO - Data Transfer Objects
    // What comes from frontend

    public record RegisterDto(
        string Email,
        string Password,
        string? PhoneNumber
    );

    public record LoginDto(
        string Email,
        string Password,
        string Channel
    );

    public record Verify2FaDto(
        string FlowId,
        string Code,
        string Channel
    );

    public record ForgotPasswordDto(
        string Email
    );

    public record ResetPasswordDto(
        string Email,
        string Token,
        string NewPassword
    );
}