using FinShark.Application.Dtos;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Net;

namespace FinShark.API.Middleware;

/// <summary>
/// Global exception handling middleware
/// Catches unhandled exceptions and returns consistent error responses
/// Logs exceptions for debugging and audit trail
/// </summary>
public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException vex)
        {
            _logger.LogWarning(vex, "Validation exception occurred");
            await HandleValidationExceptionAsync(context, vex);
        }
        catch (KeyNotFoundException knfex)
        {
            _logger.LogWarning(knfex, "Resource not found");
            await HandleNotFoundExceptionAsync(context, knfex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var response = ApiResponse<object>.FailureResponse("Validation failed", 
            new[] { exception.Message });

        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleNotFoundExceptionAsync(HttpContext context, KeyNotFoundException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;

        var response = ApiResponse<object>.FailureResponse(exception.Message);

        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = ApiResponse<object>.FailureResponse(
            "An unexpected error occurred. Please contact support.",
            new[] { exception.Message });

        return context.Response.WriteAsJsonAsync(response);
    }
}
