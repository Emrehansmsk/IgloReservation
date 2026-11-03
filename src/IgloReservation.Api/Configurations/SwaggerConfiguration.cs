using System.Reflection;
using Microsoft.OpenApi;

namespace IgloReservation.Api.Configurations;

public static class SwaggerConfiguration
{
    public const string SecuritySchemeId = "Bearer";

    public static void AddIgloSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            // Avoid Swagger UI uuid quirks: treat Guid as plain string in the document.
            options.MapType<Guid>(() => new OpenApiSchema { Type = JsonSchemaType.String });
            options.MapType<Guid?>(() => new OpenApiSchema { Type = JsonSchemaType.String });

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "IgloReservation API",
                Version = "v1",
                Description =
                    "SGK Üsküdar Iglo reservation API. " +
                    "Roles: **Admin** (full CRUD on reservations/units/users), **User** (read all reservations/units; no writes). " +
                    "Authenticate via `POST /api/auth/login` or `register`, then click **Authorize** and paste the JWT " +
                    "(Swagger sends `Authorization: Bearer <token>`)."
            });

            options.AddSecurityDefinition(SecuritySchemeId, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description =
                    "JWT Bearer token. Paste the access token only (without the 'Bearer ' prefix). " +
                    "Obtain a token from /api/auth/login. Role claims in the token enforce Admin vs User access."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(SecuritySchemeId, document)] = []
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }

            options.TagActionsBy(api =>
            {
                if (api.GroupName is { Length: > 0 } group)
                {
                    return [group];
                }

                var controller = api.ActionDescriptor.RouteValues.TryGetValue("controller", out var name)
                    ? name
                    : "API";
                return [controller ?? "API"];
            });
        });
    }

    public static void UseIgloSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            // Relative to the Swagger UI route so the document loads reliably.
            options.SwaggerEndpoint("v1/swagger.json", "IgloReservation API v1");
            options.DocumentTitle = "IgloReservation API";
            options.DisplayRequestDuration();
        });
    }
}
