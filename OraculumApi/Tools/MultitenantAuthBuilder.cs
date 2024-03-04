using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

public class MultitenantAuthBuilder
{
    private readonly IServiceCollection _serviceCollection;
    private readonly IConfiguration _configuration;
    public MultitenantAuthBuilder(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        _serviceCollection = serviceCollection;
        _configuration = configuration;
    }

    public void AddMultitenantAuthentication()
    {
        var authenticationBuilder = _serviceCollection.AddAuthentication(options =>
        {
            options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer();

        foreach (var tenantConfig in _configuration.GetSection("Tenants").GetChildren())
        {
            authenticationBuilder.AddJwtBearer(tenantConfig.Key, options =>
            {
                options.MetadataAddress = tenantConfig["Security:OIDC:Authority"] + "/.well-known/openid-configuration";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudience = tenantConfig["Security:OIDC:Audience"]
                };
            });
        }
    }

    public void AddMultitenantAuthorization()
    {
        var authorizationBuilder = _serviceCollection.AddAuthorization();
        var allowAnonymous = _configuration.GetValue<bool>("AllowAnonymous");

        authorizationBuilder.AddAuthorization(options =>
        {
            string[] roles = { "sysadmin", "backoffice", "frontoffice" };

            foreach (var tenantConfig in _configuration.GetSection("Tenants").GetChildren())
            {
                foreach (var role in roles)
                {
                    options.AddPolicy(AuthorizationPolicyNameRetriever.GetName(role, tenantConfig.Key), policy => AuthorizationAssertionRetriever.retrieveAssertion(
                        policy.AddAuthenticationSchemes(tenantConfig.Key), role, tenantConfig, allowAnonymous
                    ));
                }
            }
        });
    }
}