using System.Text.Json;
using plantilla_aspnet_core_api.Application;
using plantilla_aspnet_core_api.Domain;

namespace plantilla_aspnet_core_api.Infrastructure;

public sealed class RepositorioOrganizaciones(ClienteRestSupabase supabase) : IRepositorioOrganizaciones
{
    public async Task<IReadOnlyList<Organizacion>> ObtenerParaUsuarioAsync(Guid usuarioId, CancellationToken ct)
    {
        using var document = await supabase.GetAsync("organizaciones",
            $"select=id,nombre,creado_en,miembros_organizacion!inner(usuario_id)&miembros_organizacion.usuario_id=eq.{usuarioId}&order=nombre", ct);
        return document.RootElement.EnumerateArray().Select(x => new Organizacion
        {
            Id = x.GetProperty("id").GetGuid(), Nombre = x.GetProperty("nombre").GetString()!,
            CreadoEn = x.GetProperty("creado_en").GetDateTimeOffset()
        }).ToList();
    }
}

public sealed class RepositorioRoles(ClienteRestSupabase supabase) : IRepositorioRoles
{
    public async Task<IReadOnlyList<Rol>> ObtenerParaOrganizacionAsync(Guid organizacionId, CancellationToken ct)
    {
        using var document = await supabase.GetAsync("roles",
            $"select=id,nombre,descripcion,creado_en,miembros_organizacion!inner(organizacion_id)&miembros_organizacion.organizacion_id=eq.{organizacionId}&order=nombre", ct);
        return document.RootElement.EnumerateArray().Select(x => new Rol
        {
            Id = x.GetProperty("id").GetGuid(), Nombre = x.GetProperty("nombre").GetString()!,
            Descripcion = x.TryGetProperty("descripcion", out var description) && description.ValueKind != JsonValueKind.Null ? description.GetString() : null,
            CreadoEn = x.GetProperty("creado_en").GetDateTimeOffset()
        }).ToList();
    }
}

public sealed class RepositorioMiembros(ClienteRestSupabase supabase) : IRepositorioMiembros
{
    public async Task AsignarRolAsync(Guid organizacionId, Guid usuarioId, Guid rolId, CancellationToken ct)
    {
        await supabase.PostAsync("miembros_organizacion", new
        {
            organizacion_id = organizacionId, usuario_id = usuarioId, rol_id = rolId,
            creado_en = DateTimeOffset.UtcNow
        }, ct, "resolution=merge-duplicates,return=minimal");
    }
}

public sealed class RepositorioAuditoria(ClienteRestSupabase supabase) : IRepositorioAuditoria
{
    public async Task<IReadOnlyList<RegistroAuditoria>> ObtenerParaOrganizacionAsync(Guid organizacionId, CancellationToken ct)
    {
        using var document = await supabase.GetAsync("registros_auditoria",
            $"select=id,usuario_id,organizacion_id,accion,recurso,recurso_id,detalles,creado_en&organizacion_id=eq.{organizacionId}&order=creado_en.desc", ct);
        return document.RootElement.EnumerateArray().Select(x => new RegistroAuditoria
        {
            Id = x.GetProperty("id").GetGuid(), UsuarioId = x.GetProperty("usuario_id").GetGuid(),
            OrganizacionId = x.GetProperty("organizacion_id").GetGuid(), Accion = x.GetProperty("accion").GetString()!,
            Recurso = x.GetProperty("recurso").GetString()!, RecursoId = x.TryGetProperty("recurso_id", out var resourceId) && resourceId.ValueKind != JsonValueKind.Null ? resourceId.GetGuid() : null,
            Detalles = x.GetProperty("detalles").GetRawText(), CreadoEn = x.GetProperty("creado_en").GetDateTimeOffset()
        }).ToList();
    }
}

public sealed class ServicioAuditoria(
    ClienteRestSupabase supabase,
    IServicioUsuarioActual currentUser,
    IServicioContextoOrganizacion tenant) : IServicioAuditoria
{
    public async Task RegistrarAsync(string accion, string recurso, Guid? recursoId, object detalles, CancellationToken ct = default)
    {
        if (!Guid.TryParse(currentUser.UsuarioId, out var userId) || tenant.OrganizacionId is not Guid organizationId)
            throw new InvalidOperationException("No hay usuario u organización válida para registrar la auditoría.");

        await supabase.PostAsync("registros_auditoria", new
        {
            id = Guid.NewGuid(), usuario_id = userId, organizacion_id = organizationId,
            accion, recurso, recurso_id = recursoId, detalles, creado_en = DateTimeOffset.UtcNow
        }, ct);
    }
}
