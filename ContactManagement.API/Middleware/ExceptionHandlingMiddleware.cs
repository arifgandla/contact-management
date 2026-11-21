using System.Net;
using System.Text.Json;
using ContactManagement.Core.DTOs.Common;
using ContactManagement.Core.Exceptions;

namespace ContactManagement.API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
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
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = MapException(exception);

        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception while processing {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception, "Request {Method} {Path} failed: {Message}",
                context.Request.Method, context.Request.Path, message);
        }

        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse.Fail(message, errors);
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private (int StatusCode, string Message, IReadOnlyList<string>? Errors) MapException(Exception exception) =>
        exception switch
        {
            AppException appException =>
                (appException.StatusCode, appException.Message, appException.Errors.Count > 0 ? appException.Errors : null),
            UnauthorizedAccessException =>
                ((int)HttpStatusCode.Unauthorized, "Authentication is required.", null),
            OperationCanceledException =>
                ((int)HttpStatusCode.BadRequest, "The request was cancelled.", null),
            _ when _environment.IsProduction() =>
                ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.", null),
            _ =>
                ((int)HttpStatusCode.InternalServerError, exception.Message, null)
        };
}
