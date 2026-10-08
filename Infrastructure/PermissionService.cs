using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using plantilla_aspnet_core_api.Application;

namespace plantilla_aspnet_core_api.Infrastructure;

public sealed class PermissionService(
    SupabaseRestClient supabase,
    IMemoryCache cache,
    IOptions<CacheSettings> cacheSettings) : IPermissionService
{
    public async Task<IEnumerable<string>> GetUserPermissionsAsync(string usuarioId, Guid organizacionId, CancellationToken ct = default)
    {
        var key = $"perm:{usuarioId}:{organizacionId}";
        if (cache.TryGetValue(key, out string[]? cached) && cached is not null)
            return cached;

        if (!Guid.TryParse(usuarioId, out var userId))
            return Array.Empty<string>();

        var members = await supabase.GetAsync("miembros_organizacion",
            $"select=rol_id&usuario_id=eq.{userId}&organizacion_id=eq.{organizacionId}", ct);
        var roleIds = members.RootElement.EnumerateArray()
            .Select(x => x.GetProperty("rol_id").GetGuid()).ToArray();
        if (roleIds.Length == 0)
            return Array.Empty<string>();

        var roleFilter = string.Join(",", roleIds.Select(id => id.ToString()));
        var rolePermissions = await supabase.GetAsync("roles_permisos",
            $"select=permiso_id&rol_id=in.({roleFilter})", ct);
        var permissionIds = rolePermissions.RootElement.EnumerateArray()
            .Select(x => x.GetProperty("permiso_id").GetGuid()).Distinct().ToArray();
        if (permissionIds.Length == 0)
            return Array.Empty<string>();

        var permissionFilter = string.Join(",", permissionIds.Select(id => id.ToString()));
        var permissionRows = await supabase.GetAsync("permisos",
            $"select=nombre&id=in.({permissionFilter})", ct);
        var permissions = permissionRows.RootElement.EnumerateArray()
            .Select(x => x.GetProperty("nombre").GetString()!)
            .ToArray();

        cache.Set(key, permissions, TimeSpan.FromMinutes(Math.Max(1, cacheSettings.Value.PermissionCacheExpirationMinutes)));
        return permissions;
    }

    public async Task<bool> HasPermissionAsync(string usuarioId, Guid organizacionId, string nombrePermiso, CancellationToken ct = default) =>
        (await GetUserPermissionsAsync(usuarioId, organizacionId, ct)).Contains(nombrePermiso, StringComparer.Ordinal);
}
