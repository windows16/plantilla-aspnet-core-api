using Microsoft.AspNetCore.Authorization;

namespace plantilla_aspnet_core_api.Application;

public sealed record RequisitoPermiso(string NombrePermiso) : IAuthorizationRequirement;
