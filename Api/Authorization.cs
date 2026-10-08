using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using plantilla_aspnet_core_api.Application;

namespace plantilla_aspnet_core_api.Api;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute(string nombrePermiso) : AuthorizeAttribute($"Permission:{nombrePermiso}");

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireTenantAttribute : Attribute;

public sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith("Permission:", StringComparison.Ordinal))
            return base.GetPolicyAsync(policyName);

        var permission = policyName["Permission:".Length..];
        var policy = new AuthorizationPolicyBuilder().AddAuthenticationSchemes("Bearer")
            .RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(permission)).Build();
        return Task.FromResult<AuthorizationPolicy?>(policy);
    }
}
