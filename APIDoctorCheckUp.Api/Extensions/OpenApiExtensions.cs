using Scalar.AspNetCore;

namespace APIDoctorCheckUp.Api.Extensions;

public static class OpenApiExtensions
{
    /// <summary>
    /// Registers the built-in .NET 10 OpenAPI document generator.
    /// Replaces Swashbuckle, which was removed from the default template in .NET 9.
    /// The raw JSON document is served at /openapi/v1.json.
    /// </summary>
    public static IServiceCollection AddOpenApiDocs(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, ct) =>
            {
                document.Info.Title       = "APIDoctorCheckUp";
                document.Info.Version     = "v1";
                document.Info.Description = "Real-time REST API health monitoring dashboard";
                return Task.CompletedTask;
            });
        });

        return services;
    }

    /// <summary>
    /// Maps the OpenAPI JSON endpoint and the Scalar interactive UI.
    /// Scalar reads the JSON document and renders a modern browser UI at /scalar/v1.
    /// Intentionally restricted to Development so the docs are not publicly
    /// browsable on the production Render.com deployment.
    /// </summary>
    public static WebApplication UseOpenApiDocs(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        return app;
    }
}
