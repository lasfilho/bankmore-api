using BankMore.ContaCorrente.API.DTOs;
using BankMore.ContaCorrente.Domain.Exceptions;

namespace BankMore.ContaCorrente.API.Middlewares;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedBusinessException ex)
        {
            // Login: 401
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new ApiError(ex.Message, ex.Type));
        }
        catch (UnauthorizedAccessException ex)
        {
            // Token inválido/expirado: 403
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new ApiError(ex.Message, "USER_UNAUTHORIZED"));
        }
        catch (BusinessException ex)
        {
            // Validações: 400
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new ApiError(ex.Message, ex.Type));
        }
    }
}
