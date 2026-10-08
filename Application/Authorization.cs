using Microsoft.AspNetCore.Authorization;

namespace plantilla_aspnet_core_api.Application;

public sealed record PermissionRequirement(string NombrePermiso) : IAuthorizationRequirement;
