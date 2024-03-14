using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IConfiguration _configuration;

    public ConfigureSwaggerOptions(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(SwaggerGenOptions options)
    {
        var tenantsSection = _configuration.GetSection("Tenants");
        foreach (var tenant in tenantsSection.GetChildren())
        {
            var tenantId = tenant.Key;

            var authority = tenant[$"Security:OIDC:Authority"];
            var clientId = tenant[$"Security:OIDC:ClientId"];

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.OAuth2,
                In = ParameterLocation.Header,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{authority}/protocol/openid-connect/auth"),
                        TokenUrl = new Uri($"{authority}/protocol/openid-connect/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            {"openid", "Open ID"},
                            {"profile", "Profile"},
                            {"offline_access", "Offline Access"}
                        }
                    }
                }
            };

            options.AddSecurityDefinition($"OAuth2_{tenantId}", securityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = $"OAuth2_{tenantId}" }
                    },
                    new[] { "openid" }
                }
            });
        }
    }
}
