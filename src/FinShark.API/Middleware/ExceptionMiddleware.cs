using FinShark.Application.Dtos;
using FinShark.Domain.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

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
        catch (StockAlreadyExistsException saex)
        {
            _logger.LogWarning(saex, "Conflict occurred");
            await HandleConflictExceptionAsync(context, saex);
        }
        catch (StockNotFoundException snfex)
        {
            _logger.LogWarning(snfex, "Resource not found");
            await HandleNotFoundExceptionAsync(context, snfex);
        }
        catch (CommentNotFoundException cnfex)
        {
            _logger.LogWarning(cnfex, "Resource not found");
            await HandleNotFoundExceptionAsync(context, cnfex);
        }
        catch (FinSharkException fex)
        {
            _logger.LogWarning(fex, "Domain exception occurred");
            await HandleDomainExceptionAsync(context, fex);
        }
        catch (KeyNotFoundException knfex)
        {
            _logger.LogWarning(knfex, "Resource not found");
            await HandleNotFoundExceptionAsync(context, knfex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context);
        }
    }

    private static Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var errors = exception.Errors
            .Select(e => e.ErrorMessage)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Distinct()
            .ToArray();

        var response = ApiResponse<object>.FailureResponse("Validation failed", errors);

        return WriteJsonAsync(context, response);
    }

    private static Task HandleNotFoundExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;

        var response = ApiResponse<object>.FailureResponse(exception.Message);

        return WriteJsonAsync(context, response);
    }

    private static Task HandleConflictExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.Conflict;

        var response = ApiResponse<object>.FailureResponse(exception.Message);

        return WriteJsonAsync(context, response);
    }

    private static Task HandleDomainExceptionAsync(HttpContext context, FinSharkException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var response = ApiResponse<object>.FailureResponse(exception.Message);

        return WriteJsonAsync(context, response);
    }

    private static Task HandleExceptionAsync(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = ApiResponse<object>.FailureResponse(
            "An unexpected error occurred. Please contact support.");

        return WriteJsonAsync(context, response);
    }

    private static Task WriteJsonAsync(HttpContext context, ApiResponse<object> response)
    {
        var payload = JsonConvert.SerializeObject(response, JsonSettings);
        return context.Response.WriteAsync(payload, context.RequestAborted);
    }
}