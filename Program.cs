using Microsoft.OpenApi.Models;
using plantilla_aspnet_core_api.Api;
using plantilla_aspnet_core_api.Application;
using plantilla_aspnet_core_api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
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
    options.OperationFilter<OrganizationHeaderOperationFilter>();
});

builder.Services.Configure<SupabaseOptions>(builder.Configuration.GetSection("Supabase"));
builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("CacheSettings"));
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSupabaseAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ITenantContextService, TenantContextService>();

builder.Services.AddHttpClient<SupabaseRestClient>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SupabaseOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.Url) || string.IsNullOrWhiteSpace(options.ServiceRoleKey))
        throw new InvalidOperationException("Supabase:Url y Supabase:ServiceRoleKey son obligatorios.");

    client.BaseAddress = new Uri($"{options.Url.TrimEnd('/')}/rest/v1/");
    client.DefaultRequestHeaders.Add("apikey", options.ServiceRoleKey);
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ServiceRoleKey);
});
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<StandardProblemDetailsMiddleware>();
app.UseRouting();
app.UseMiddleware<TenantContextMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
