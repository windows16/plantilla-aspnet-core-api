namespace plantilla_aspnet_core_api.Domain;

public sealed class Organizacion
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTimeOffset CreadoEn { get; set; }
}

public sealed class Rol
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
}

public sealed class Permiso
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
}

public sealed class RolPermiso
{
    public Guid RolId { get; set; }
    public Guid PermisoId { get; set; }
}

public sealed class MiembroOrganizacion
{
    public Guid OrganizacionId { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid RolId { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
}

public sealed class RegistroAuditoria
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid OrganizacionId { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Recurso { get; set; } = string.Empty;
    public Guid? RecursoId { get; set; }
    public string Detalles { get; set; } = "{}";
    public DateTimeOffset CreadoEn { get; set; }
}
