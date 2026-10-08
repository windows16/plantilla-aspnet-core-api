using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using plantilla_aspnet_core_api.Application;

namespace plantilla_aspnet_core_api.Infrastructure;

public sealed class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    private ClaimsPrincipal User => accessor.HttpContext?.User ?? new ClaimsPrincipal();
    public string? UsuarioId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
    public string? Email => User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
    public bool IsAuthenticated => User.Identity?.IsAuthenticated == true;
}

public sealed class TenantContextService(IHttpContextAccessor accessor) : ITenantContextService
{
    public Guid? OrganizacionId
    {
        get
        {
            var value = accessor.HttpContext?.Request.Headers["X-Organizacion-Id"].FirstOrDefault();
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
