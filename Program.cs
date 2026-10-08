using Microsoft.OpenApi.Models;
using plantilla_aspnet_core_api.Api;
using plantilla_aspnet_core_api.Application;
using plantilla_aspnet_core_api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization", Type = SecuritySchemeType.Http, Scheme = "bearer",
        BearerFormat = "JWT", In = ParameterLocation.Header,
        Description = "JWT de Supabase Auth."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = Array.Empty<string>()
    });
    options.OperationFilter<FiltroCabeceraOrganizacion>();
});

builder.Services.Configure<OpcionesSupabase>(builder.Configuration.GetSection("Supabase"));
builder.Services.Configure<ConfiguracionCache>(builder.Configuration.GetSection("ConfiguracionCache"));
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSupabaseAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddExceptionHandler<ManejadorExcepcionesProblemDetails>();
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider, ProveedorPoliticasPermisos>();
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, ManejadorAutorizacionPermisos>();
builder.Services.AddScoped<IServicioUsuarioActual, ServicioUsuarioActual>();
builder.Services.AddScoped<IServicioContextoOrganizacion, ServicioContextoOrganizacion>();

builder.Services.AddHttpClient<ClienteRestSupabase>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpcionesSupabase>>().Value;
    if (string.IsNullOrWhiteSpace(options.Url) || string.IsNullOrWhiteSpace(options.ServiceRoleKey))
        throw new InvalidOperationException("Supabase:Url y Supabase:ServiceRoleKey son obligatorios.");

    client.BaseAddress = new Uri($"{options.Url.TrimEnd('/')}/rest/v1/");
    client.DefaultRequestHeaders.Add("apikey", options.ServiceRoleKey);
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ServiceRoleKey);
});
builder.Services.AddScoped<IServicioPermisos, ServicioPermisos>();
builder.Services.AddScoped<IServicioAuditoria, ServicioAuditoria>();
builder.Services.AddScoped<IRepositorioOrganizaciones, RepositorioOrganizaciones>();
builder.Services.AddScoped<IRepositorioRoles, RepositorioRoles>();
builder.Services.AddScoped<IRepositorioMiembros, RepositorioMiembros>();
builder.Services.AddScoped<IRepositorioAuditoria, RepositorioAuditoria>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<MiddlewareErroresEstandar>();
app.UseRouting();
app.UseMiddleware<MiddlewareContextoOrganizacion>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
