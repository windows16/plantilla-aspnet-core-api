using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace plantilla_aspnet_core_api.Api;

public sealed class MiddlewareContextoOrganizacion(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<RequiereOrganizacionAttribute>() is not null)
        {
            var value = context.Request.Headers["X-Organizacion-Id"].FirstOrDefault();
            if (!Guid.TryParse(value, out _))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new RespuestaProblema(
                    "https://httpstatuses.com/400", "Bad Request", 400,
                    "El encabezado X-Organizacion-Id es obligatorio y debe ser un UUID válido.", context.TraceIdentifier));
                return;
            }
        }
        await next(context);
    }
}

public sealed class MiddlewareErroresEstandar(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);
        if (context.Response.HasStarted || context.Response.StatusCode is not (401 or 403))
            return;

        context.Response.ContentType = "application/problem+json";
        var title = context.Response.StatusCode == 401 ? "Unauthorized" : "Forbidden";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new RespuestaProblema(
            $"https://httpstatuses.com/{context.Response.StatusCode}", title, context.Response.StatusCode,
            context.Response.StatusCode == 401 ? "Se requiere autenticación válida." : "No tiene el permiso requerido.",
            context.TraceIdentifier)));
    }
}

public sealed class ManejadorExcepcionesProblemDetails(
    IHostEnvironment environment,
    ILogger<ManejadorExcepcionesProblemDetails> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Error no controlado durante la solicitud {TraceId}", httpContext.TraceIdentifier);
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(new RespuestaProblema(
            "https://httpstatuses.com/500", "Internal Server Error", 500,
            environment.IsDevelopment() ? exception.Message : "Ocurrió un error interno.",
            httpContext.TraceIdentifier), cancellationToken);
        return true;
    }
}

public sealed record RespuestaProblema(string Type, string Title, int Status, string Detail, string TraceId);
