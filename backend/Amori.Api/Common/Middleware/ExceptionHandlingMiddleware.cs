using System.Text.Json;
using Amori.Api.Common.Exceptions;
using Amori.Api.Common.Responses;

namespace Amori.Api.Common.Middleware;

/// <summary>
/// Catches unhandled exceptions and returns consistent JSON error responses.
/// Never exposes stack traces or internal details in production.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            NotFoundException ex =>
                (StatusCodes.Status404NotFound,
                 ApiResponse.Fail(ex.Message)),

            UnauthorizedException ex =>
                (StatusCodes.Status401Unauthorized,
                 ApiResponse.Fail(ex.Message)),

            Exceptions.ValidationException ex =>
                (StatusCodes.Status422UnprocessableEntity,
                 ApiResponse.Fail(ex.Message, ex.Errors)),

            ConflictException ex =>
                (StatusCodes.Status409Conflict,
                 ApiResponse.Fail(ex.Message)),

            _ => (StatusCodes.Status500InternalServerError,
                  ApiResponse.Fail(GetGenericMessage(exception)))
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private string GetGenericMessage(Exception exception)
    {
        // Only expose exception messages in development
        if (environment.IsDevelopment())
        {
            return exception.Message;
        }

        return "An unexpected error occurred.";
    }
}
