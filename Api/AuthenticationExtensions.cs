using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using plantilla_aspnet_core_api.Infrastructure;

namespace plantilla_aspnet_core_api.Api;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddSupabaseAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection("Supabase").Get<SupabaseOptions>() ?? new();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(jwt =>
        {
            jwt.RequireHttpsMetadata = options.RequireHttpsMetadata;
            jwt.SaveToken = false;
            jwt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(options.JwtSecret)),
                ValidateIssuer = true,
                ValidIssuer = options.Issuer,
                ValidateAudience = true,
                ValidAudience = options.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });
        return services;
    }
}
