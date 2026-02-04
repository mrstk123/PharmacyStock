using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PharmacyStock.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = GetErrorDetails(exception);

        // Log based on severity
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Handled exception ({StatusCode}): {Message}", (int)statusCode, exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ErrorResponse
        {
            StatusCode = (int)statusCode,
            Message = message,
            // Only include detailed error info in development
            Detail = _environment.IsDevelopment() ? exception.Message : null,
            TraceId = context.TraceIdentifier
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }

    private static (HttpStatusCode StatusCode, string Message) GetErrorDetails(Exception exception) => exception switch
    {
        KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
        UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message),
        ArgumentNullException => (HttpStatusCode.BadRequest, exception.Message),
        ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
        InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),
        DbUpdateConcurrencyException => (HttpStatusCode.Conflict, "The record was modified by another user. Please refresh and try again."),
        DbUpdateException dbEx when dbEx.InnerException?.Message.Contains("UNIQUE") == true
            => (HttpStatusCode.Conflict, "A record with this value already exists."),
        DbUpdateException => (HttpStatusCode.BadRequest, "Database operation failed. Please check your input."),
        _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.")
    };
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = null!;
    public string? Detail { get; set; }
    public string? TraceId { get; set; }
}
