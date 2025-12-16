using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SendGrid.Helpers.Errors.Model;
using Serilog;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Choose status + machine-readable "type"
        var (status, type, title, detail) = MapException(exception);

        // Log appropriately
        if (status >= 500)
            Log.Error(exception, "Unhandled server error: {Message}", exception.Message);
        else
            Log.Warning(exception, "Handled error {Status}: {Message}", status, exception.Message);

        // Build RFC 7807 ProblemDetails
        var problem = new ProblemDetails
        {
            Type   = type,
            Title  = title,
            Status = status,
            Detail = detail,
            Instance = context.Request?.Path.Value
        };

        // Add trace id for correlation
        // Surface validation errors from DataAnnotations.ValidationException (if present)
        if (exception is System.ComponentModel.DataAnnotations.ValidationException dae && dae.ValidationResult != null)
        {
            var memberNames = dae.ValidationResult.MemberNames?.ToArray() ?? [];
            var errors = memberNames.Length > 0
                ? memberNames.ToDictionary(m => m, m => new[] { dae.ValidationResult.ErrorMessage ?? string.Empty })
                : new Dictionary<string, string[]> { { string.Empty, new[] { dae.ValidationResult.ErrorMessage ?? string.Empty } } };
            problem.Extensions["errors"] = errors;
        }

        context.Response.StatusCode  = status;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true; 
    }

    private static (int status, string type, string title, string detail) MapException(Exception ex)
    {
        // 401 Unauthorized
        if (ex is AuthenticationException ||
            ex is UnauthorizedAccessException ||
            ex is SecurityTokenException)
        {
            var expired = ex is SecurityTokenExpiredException;
            return (
                StatusCodes.Status401Unauthorized,
                expired
                    ? "https://httpstatuses.com/401#token-expired"
                    : "https://httpstatuses.com/401",
                expired ? "Unauthorized (token expired)" : "Unauthorized",
                expired ? "Your access token has expired." : ex.Message
            );
        }

        // 403 Forbidden
        if (ex is ForbiddenException) // <- custom if you have one
        {
            return (
                StatusCodes.Status403Forbidden,
                "https://httpstatuses.com/403",
                "Forbidden",
                ex.Message
            );
        }

        // 404 Not Found
        if (ex is KeyNotFoundException || ex is NotFoundException) // custom optional
        {
            return (
                StatusCodes.Status404NotFound,
                "https://httpstatuses.com/404",
                "Not Found",
                ex.Message
            );
        }

        // 400 Bad Request (validation, argument issues)
        if (ex is ValidationException ||
            ex is ArgumentException ||
            ex is FormatException)
        {
            return (
                StatusCodes.Status400BadRequest,
                "https://httpstatuses.com/400",
                "Bad Request",
                ex.Message
            );
        }

        // default 500
        return (
            StatusCodes.Status500InternalServerError,
            "https://httpstatuses.com/500",
            "Server Error",
            "An unexpected error occurred."
        );
    }
}
