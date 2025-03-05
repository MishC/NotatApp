using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async Task<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        context.Response.ContentType = "application/json";
        var statusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest, // Validation Error
            KeyNotFoundException => StatusCodes.Status404NotFound,  // Not Found
            _ => StatusCodes.Status500InternalServerError           // General Server Error
        };

        if (exception is ValidationException)
        {
            Log.Warning("Validation Error: {Message}", exception.Message);
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine($"Validation Issue: {exception.Message}");
            Console.ResetColor();
        }
        else
        {
            Log.Error("❌ Error: {Message}", exception.Message);
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"❌ ERROR: {exception.Message}");
            Console.ResetColor();
        }

         var problemDetails = new ProblemDetails
        {
            Title = "An error occurred",
            Status = statusCode,
            Detail = exception.Message
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        return true;
    }

    ValueTask<bool> IExceptionHandler.TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
