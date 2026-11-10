using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using PruebaTecnica.Common.Middleware;

namespace PruebaTecnica.Common.OpenApi;

// Declara el header X-API-KEY en el documento OpenAPI para que Swagger UI muestre el botón "Authorize".
public class ApiKeySecurityTransformer : IOpenApiDocumentTransformer
{
    private const string SchemeName = "ApiKey";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = ApiKeyMiddleware.HeaderName,
            Description = "API Key requerida en todos los endpoints."
        };

        document.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(SchemeName, document)] = []
            }
        ];

        return Task.CompletedTask;
    }
}
