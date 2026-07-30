using FluentValidation;
using KromicStore.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace KromicStore.API.Middleware;

/// <summary>
/// Global exception handling middleware.
/// Maps every known exception type to an RFC 7807 ProblemDetails response.
/// Stack traces are never sent to clients.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.TraceIdentifier;

        var (status, title, detail, errors) = exception switch
        {
            ValidationException ve => (
                HttpStatusCode.BadRequest,
                "Validation Failure",
                "One or more validation failures have occurred.",
                ve.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
                as IDictionary<string, string[]>),

            AuthenticationException => (
                HttpStatusCode.Unauthorized,
                "Authentication Failed",
                exception.Message,
                (IDictionary<string, string[]>?)null),

            EmailNotVerifiedException => (
                HttpStatusCode.Forbidden,
                "Email Not Verified",
                exception.Message,
                (IDictionary<string, string[]>?)null),

            AccountLockedException => (
                (HttpStatusCode)423,
                "Account Locked",
                exception.Message,
                (IDictionary<string, string[]>?)null),

            NotFoundException => (
                HttpStatusCode.NotFound,
                "Not Found",
                exception.Message,
                (IDictionary<string, string[]>?)null),

            ConflictException => (
                HttpStatusCode.Conflict,
                "Conflict",
                exception.Message,
                (IDictionary<string, string[]>?)null),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "Unauthorized",
                exception.Message,
                (IDictionary<string, string[]>?)null),

            _ => (
                HttpStatusCode.InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred. Please try again later.",
                (IDictionary<string, string[]>?)null)
        };

        // Log at appropriate level
        if (status == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception on {Path}", context.Request.Path);
        else
            _logger.LogWarning(exception, "Handled exception {ExceptionType} on {Path}", exception.GetType().Name, context.Request.Path);

        var problem = new ProblemDetails
        {
            Status   = (int)status,
            Title    = title,
            Detail   = detail,
            Instance = context.Request.Path,
            Extensions =
            {
                ["correlationId"] = correlationId,
                ["traceId"]       = context.TraceIdentifier
            }
        };

        if (errors is not null)
            problem.Extensions["errors"] = errors;

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode  = (int)status;

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(json);
    }
}
