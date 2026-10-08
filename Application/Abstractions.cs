using plantilla_aspnet_core_api.Domain;

namespace plantilla_aspnet_core_api.Application;

public interface ICurrentUserService
{
    string? UsuarioId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}

public interface ITenantContextService
{
    Guid? OrganizacionId { get; }
}

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string usuarioId, Guid organizacionId, string nombrePermiso, CancellationToken ct = default);
    Task<IEnumerable<string>> GetUserPermissionsAsync(string usuarioId, Guid organizacionId, CancellationToken ct = default);
}

public interface IAuditService
{
    Task LogAsync(string accion, string recurso, Guid? recursoId, object detalles, CancellationToken ct = default);
}

public interface IOrganizationRepository
{
    Task<IReadOnlyList<Organizacion>> GetForUserAsync(Guid usuarioId, CancellationToken ct);
}

public interface IRoleRepository
{
    Task<IReadOnlyList<Rol>> GetForTenantAsync(Guid organizacionId, CancellationToken ct);
}

public interface IMemberRepository
{
    Task AssignRoleAsync(Guid organizacionId, Guid usuarioId, Guid rolId, CancellationToken ct);
}

public interface IAuditRepository
{
    Task<IReadOnlyList<RegistroAuditoria>> GetForTenantAsync(Guid organizacionId, CancellationToken ct);
}
