using plantilla_aspnet_core_api.Domain;

namespace plantilla_aspnet_core_api.Application;

public interface IServicioUsuarioActual
{
    string? UsuarioId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}

public interface IServicioContextoOrganizacion
{
    Guid? OrganizacionId { get; }
}

public interface IServicioPermisos
{
    Task<bool> TienePermisoAsync(string usuarioId, Guid organizacionId, string nombrePermiso, CancellationToken ct = default);
    Task<IEnumerable<string>> ObtenerPermisosUsuarioAsync(string usuarioId, Guid organizacionId, CancellationToken ct = default);
}

public interface IServicioAuditoria
{
    Task RegistrarAsync(string accion, string recurso, Guid? recursoId, object detalles, CancellationToken ct = default);
}

public interface IRepositorioOrganizaciones
{
    Task<IReadOnlyList<Organizacion>> ObtenerParaUsuarioAsync(Guid usuarioId, CancellationToken ct);
}

public interface IRepositorioRoles
{
    Task<IReadOnlyList<Rol>> ObtenerParaOrganizacionAsync(Guid organizacionId, CancellationToken ct);
}

public interface IRepositorioMiembros
{
    Task AsignarRolAsync(Guid organizacionId, Guid usuarioId, Guid rolId, CancellationToken ct);
}

public interface IRepositorioAuditoria
{
    Task<IReadOnlyList<RegistroAuditoria>> ObtenerParaOrganizacionAsync(Guid organizacionId, CancellationToken ct);
}
