using Microsoft.AspNetCore.Authorization;
using plantilla_aspnet_core_api.Application;

namespace plantilla_aspnet_core_api.Infrastructure;

public sealed class PermissionAuthorizationHandler(
    IPermissionService permissions,
    ICurrentUserService currentUser,
    ITenantContextService tenant) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (!currentUser.IsAuthenticated || !Guid.TryParse(currentUser.UsuarioId, out var userId) || tenant.OrganizacionId is not Guid organizationId)
            return;

        if (await permissions.HasPermissionAsync(userId.ToString(), organizationId, requirement.NombrePermiso, context.Resource is HttpContext http
            ? http.RequestAborted
            : CancellationToken.None))
            context.Succeed(requirement);
    }
}
