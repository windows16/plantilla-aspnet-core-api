using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using plantilla_aspnet_core_api.Api;
using plantilla_aspnet_core_api.Application;

namespace plantilla_aspnet_core_api.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public sealed class SaasController(
    ICurrentUserService currentUser,
    ITenantContextService tenant,
    IOrganizationRepository organizations,
    IRoleRepository roles,
    IMemberRepository members,
    IAuditRepository audits,
    IAuditService audit) : ControllerBase
{
    [HttpGet("organizaciones")]
    public async Task<ActionResult> GetOrganizations(CancellationToken ct)
    {
        if (!Guid.TryParse(currentUser.UsuarioId, out var userId))
            return Unauthorized();
        return Ok(await organizations.GetForUserAsync(userId, ct));
    }

    [HttpGet("roles")]
    [RequireTenant]
    [HasPermission("roles:gestionar")]
    public async Task<ActionResult> GetRoles(CancellationToken ct) =>
        Ok(await roles.GetForTenantAsync(tenant.OrganizacionId!.Value, ct));

    [HttpPost("miembros/asignar-rol")]
    [RequireTenant]
    [HasPermission("usuarios:gestionar")]
    public async Task<ActionResult> AssignRole([FromBody] AssignRoleRequest request, CancellationToken ct)
    {
        await members.AssignRoleAsync(tenant.OrganizacionId!.Value, request.UsuarioId, request.RolId, ct);
        await audit.LogAsync("asignar_rol", "miembros_organizacion", request.UsuarioId, new { rol = request.RolId });
        return NoContent();
    }

    [HttpGet("auditoria")]
    [RequireTenant]
    [HasPermission("auditoria:leer")]
    public async Task<ActionResult> GetAudit(CancellationToken ct) =>
        Ok(await audits.GetForTenantAsync(tenant.OrganizacionId!.Value, ct));
}

public sealed record AssignRoleRequest(Guid UsuarioId, Guid RolId);
