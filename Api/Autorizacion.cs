using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using plantilla_aspnet_core_api.Application;

namespace plantilla_aspnet_core_api.Api;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequierePermisoAttribute(string nombrePermiso) : AuthorizeAttribute($"Permiso:{nombrePermiso}");

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequiereOrganizacionAttribute : Attribute;

public sealed class ProveedorPoliticasPermisos(IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith("Permiso:", StringComparison.Ordinal))
            return base.GetPolicyAsync(policyName);

        var permission = policyName["Permiso:".Length..];
        var policy = new AuthorizationPolicyBuilder().AddAuthenticationSchemes("Bearer")
            .RequireAuthenticatedUser().AddRequirements(new RequisitoPermiso(permission)).Build();
        return Task.FromResult<AuthorizationPolicy?>(policy);
    }
}
