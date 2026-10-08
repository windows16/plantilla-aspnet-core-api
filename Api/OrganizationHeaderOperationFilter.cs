using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace plantilla_aspnet_core_api.Api;

public sealed class OrganizationHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor.EndpointMetadata.Any(m => m is RequireTenantAttribute))
        {
            operation.Parameters ??= [];
            if (!operation.Parameters.Any(p => p.Name == "X-Organizacion-Id"))
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "X-Organizacion-Id", In = ParameterLocation.Header, Required = true,
                    Schema = new OpenApiSchema { Type = "string", Format = "uuid" },
                    Description = "Organización activa para el contexto tenant."
                });
        }
    }
}
