using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using plantilla_aspnet_core_api.Application;

namespace plantilla_aspnet_core_api.Infrastructure;

public sealed class ServicioPermisos(
    ClienteRestSupabase supabase,
    IMemoryCache cache,
    IOptions<ConfiguracionCache> cacheSettings) : IServicioPermisos
{
    public async Task<IEnumerable<string>> ObtenerPermisosUsuarioAsync(string usuarioId, Guid organizacionId, CancellationToken ct = default)
    {
        var key = $"perm:{usuarioId}:{organizacionId}";
        if (cache.TryGetValue(key, out string[]? cached) && cached is not null)
            return cached;

        if (!Guid.TryParse(usuarioId, out var idUsuario))
            return Array.Empty<string>();

        using var miembros = await supabase.GetAsync("miembros_organizacion",
            $"select=rol_id&usuario_id=eq.{idUsuario}&organizacion_id=eq.{organizacionId}", ct);
        var idsRoles = miembros.RootElement.EnumerateArray()
            .Select(x => x.GetProperty("rol_id").GetGuid()).ToArray();
        if (idsRoles.Length == 0)
            return Array.Empty<string>();

        var filtroRoles = string.Join(",", idsRoles.Select(id => id.ToString()));
        using var permisosRoles = await supabase.GetAsync("roles_permisos",
            $"select=permiso_id&rol_id=in.({filtroRoles})", ct);
        var idsPermisos = permisosRoles.RootElement.EnumerateArray()
            .Select(x => x.GetProperty("permiso_id").GetGuid()).Distinct().ToArray();
        if (idsPermisos.Length == 0)
            return Array.Empty<string>();

        var filtroPermisos = string.Join(",", idsPermisos.Select(id => id.ToString()));
        using var filasPermisos = await supabase.GetAsync("permisos",
            $"select=nombre&id=in.({filtroPermisos})", ct);
        var permisos = filasPermisos.RootElement.EnumerateArray()
            .Select(x => x.GetProperty("nombre").GetString()!)
            .ToArray();

        cache.Set(key, permisos, TimeSpan.FromMinutes(Math.Max(1, cacheSettings.Value.ExpiracionCachePermisosMinutos)));
        return permisos;
    }

    public async Task<bool> TienePermisoAsync(string usuarioId, Guid organizacionId, string nombrePermiso, CancellationToken ct = default) =>
        (await ObtenerPermisosUsuarioAsync(usuarioId, organizacionId, ct)).Contains(nombrePermiso, StringComparer.Ordinal);
}
