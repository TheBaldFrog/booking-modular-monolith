using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

public class SecuritySchemeDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        document.Components ??= new OpenApiComponents();

        // Initialize with the correct interface type
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        var securitySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description =
                    "Enter only your JWT token, without the 'Bearer' prefix (Swagger adds it automatically).\n\nExample: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
            },
            ["ApiKey"] = new OpenApiSecurityScheme
            {
                Name = "X-API-KEY",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "Enter your API key in the text input below.\n\nExample: '12345-abcdef'",
            },
        };

        foreach (var (key, scheme) in securitySchemes)
        {
            if (!document.Components.SecuritySchemes.ContainsKey(key))
            {
                document.Components.SecuritySchemes.Add(key, scheme);
            }
        }

        // Declaring the schemes is not enough: without a security requirement Swagger UI
        // never attaches the Authorization header to requests.
        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>(),
            }
        );

        return Task.CompletedTask;
    }
}
