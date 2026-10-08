using System.Security.Claims;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using plantilla_aspnet_core_api.Infrastructure;

namespace plantilla_aspnet_core_api.Api;

public static class ExtensionesAutenticacion
{
    public static IServiceCollection AddSupabaseAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, ManejadorAutenticacionSupabase>(
                JwtBearerDefaults.AuthenticationScheme, _ => { });
        return services;
    }
}

public sealed class ManejadorAutenticacionSupabase(
    IOptionsMonitor<AuthenticationSchemeOptions> opciones,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<OpcionesSupabase> configuracion,
    IHttpClientFactory clientesHttp) : AuthenticationHandler<AuthenticationSchemeOptions>(opciones, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var encabezado = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(encabezado))
            return AuthenticateResult.NoResult();

        if (!AuthenticationHeaderValue.TryParse(encabezado, out var autorizacion) ||
            !string.Equals(autorizacion.Scheme, JwtBearerDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(autorizacion.Parameter))
            return AuthenticateResult.Fail("El encabezado Authorization debe usar el esquema Bearer.");

        var supabase = configuracion.Value;
        if (string.IsNullOrWhiteSpace(supabase.Url) || string.IsNullOrWhiteSpace(supabase.ServiceRoleKey))
            return AuthenticateResult.Fail("La configuración de Supabase está incompleta.");

        try
        {
            var cliente = clientesHttp.CreateClient();
            using var solicitud = new HttpRequestMessage(
                HttpMethod.Get, $"{supabase.Url.TrimEnd('/')}/auth/v1/user");
            solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", autorizacion.Parameter);
            solicitud.Headers.Add("apikey", supabase.ServiceRoleKey);

            using var respuesta = await cliente.SendAsync(solicitud, Context.RequestAborted);
            if (!respuesta.IsSuccessStatusCode)
                return AuthenticateResult.Fail("Supabase rechazó el token de acceso.");

            await using var contenido = await respuesta.Content.ReadAsStreamAsync(Context.RequestAborted);
            using var usuario = await JsonDocument.ParseAsync(contenido, cancellationToken: Context.RequestAborted);
            var raiz = usuario.RootElement;
            if (!raiz.TryGetProperty("id", out var id) || id.ValueKind != JsonValueKind.String ||
                !Guid.TryParse(id.GetString(), out _))
                return AuthenticateResult.Fail("Supabase no devolvió una identidad válida.");

            var identidad = new ClaimsIdentity(Scheme.Name);
            identidad.AddClaim(new Claim(ClaimTypes.NameIdentifier, id.GetString()!));
            identidad.AddClaim(new Claim("sub", id.GetString()!));
            if (raiz.TryGetProperty("email", out var email) && email.ValueKind == JsonValueKind.String)
                identidad.AddClaim(new Claim(ClaimTypes.Email, email.GetString()!));

            return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identidad), Scheme.Name));
        }
        catch (OperationCanceledException) when (Context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            Logger.LogError(exception, "No fue posible validar el token con Supabase.");
            return AuthenticateResult.Fail("No fue posible validar el token con Supabase.");
        }
        catch (JsonException exception)
        {
            Logger.LogError(exception, "Supabase devolvió una respuesta de autenticación inválida.");
            return AuthenticateResult.Fail("Supabase devolvió una respuesta de autenticación inválida.");
        }
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers.WWWAuthenticate = "Bearer";
        return Task.CompletedTask;
    }
}
