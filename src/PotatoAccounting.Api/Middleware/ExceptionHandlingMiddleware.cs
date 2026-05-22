using System.Net;
using System.Text.Json;
using PotatoAccounting.Application.Common.Exceptions;
using PotatoAccounting.Domain.Exceptions;

namespace PotatoAccounting.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        object payload;

        switch (ex)
        {
            case ValidationException ve:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                payload = new { message = ve.Message, errors = ve.Errors };
                break;

            case NotFoundException nfe:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                payload = new { message = nfe.Message };
                break;

            case ForbiddenAccessException fae:
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                payload = new { message = fae.Message };
                break;

            case DomainException de:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                payload = new { message = de.Message };
                break;

            default:
                _logger.LogError(ex, "Unhandled exception");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                payload = new { message = "حدث خطأ غير متوقع" };
                break;
        }

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        await context.Response.WriteAsync(json);
    }
}
