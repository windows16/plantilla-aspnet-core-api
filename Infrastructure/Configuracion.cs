namespace plantilla_aspnet_core_api.Infrastructure;

public sealed class OpcionesSupabase
{
    public string Url { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = "authenticated";
    public string JwtSecret { get; set; } = string.Empty;
    public string ServiceRoleKey { get; set; } = string.Empty;
    public bool RequireHttpsMetadata { get; set; } = true;
}

public sealed class ConfiguracionCache
{
    public int ExpiracionCachePermisosMinutos { get; set; } = 10;
}
