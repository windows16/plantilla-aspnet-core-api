using Microsoft.AspNetCore.Authorization;
using plantilla_aspnet_core_api.Application;

namespace plantilla_aspnet_core_api.Infrastructure;

public sealed class ManejadorAutorizacionPermisos(
    IServicioPermisos permissions,
    IServicioUsuarioActual currentUser,
    IServicioContextoOrganizacion tenant) : AuthorizationHandler<RequisitoPermiso>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RequisitoPermiso requirement)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.UsuarioId, out var userId) || tenant.OrganizacionId is not Guid organizationId)
            return;

        if (await permissions.TienePermisoAsync(userId.ToString(), organizationId, requirement.NombrePermiso, context.Resource is HttpContext http
            ? http.RequestAborted
            : CancellationToken.None))
            context.Succeed(requirement);
    }
}
