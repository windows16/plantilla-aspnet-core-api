using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using plantilla_aspnet_core_api.Api;
using plantilla_aspnet_core_api.Application;

namespace plantilla_aspnet_core_api.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public sealed class ControladorSaas(
    IServicioUsuarioActual currentUser,
    IServicioContextoOrganizacion tenant,
    IRepositorioOrganizaciones organizations,
    IRepositorioRoles roles,
    IRepositorioMiembros members,
    IRepositorioAuditoria audits,
    IServicioAuditoria audit) : ControllerBase
{
    [HttpGet("organizaciones")]
    public async Task<ActionResult> ObtenerOrganizaciones(CancellationToken ct)
    {
        if (!Guid.TryParse(currentUser.UsuarioId, out var userId))
            return Unauthorized();
        return Ok(await organizations.ObtenerParaUsuarioAsync(userId, ct));
    }

    [HttpGet("roles")]
    [RequiereOrganizacion]
    [RequierePermiso("roles:gestionar")]
    public async Task<ActionResult> ObtenerRoles(CancellationToken ct) =>
        Ok(await roles.ObtenerParaOrganizacionAsync(tenant.OrganizacionId!.Value, ct));

    [HttpPost("miembros/asignar-rol")]
    [RequiereOrganizacion]
    [RequierePermiso("usuarios:gestionar")]
    public async Task<ActionResult> AsignarRol([FromBody] SolicitudAsignacionRol request, CancellationToken ct)
    {
        await members.AsignarRolAsync(tenant.OrganizacionId!.Value, request.UsuarioId, request.RolId, ct);
        await audit.RegistrarAsync("asignar_rol", "miembros_organizacion", request.UsuarioId, new { rol = request.RolId });
        return NoContent();
    }

    [HttpGet("auditoria")]
    [RequiereOrganizacion]
    [RequierePermiso("auditoria:leer")]
    public async Task<ActionResult> ObtenerAuditoria(CancellationToken ct) =>
        Ok(await audits.ObtenerParaOrganizacionAsync(tenant.OrganizacionId!.Value, ct));
}

public sealed record SolicitudAsignacionRol(Guid UsuarioId, Guid RolId);
