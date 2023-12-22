using System.Net;
using Microsoft.AspNetCore.Authorization;

public class AuthorizationAssertionRetriever
{
    public AuthorizationPolicyBuilder retrieveAssertion(AuthorizationPolicyBuilder authorizationPolicyBuilder, ConfigurationManager configurationManager, string role)
    {

        if (configurationManager.GetSection("AllowAnonymous").Get<bool>())
        {
            return anonymousPolicy(authorizationPolicyBuilder);
        }

        var anonymousRoles = configurationManager.GetSection("AnonymousRoles").Get<string[]>();
        var isAnonymous = false;

        if (anonymousRoles != null)
        {
            isAnonymous = anonymousRoles.Contains(role);
        }

        if (isAnonymous)
        {
            return anonymousPolicy(authorizationPolicyBuilder);
        }

        return authorizationPolicyBuilder.RequireAuthenticatedUser().RequireRole(
                    configurationManager.GetSection("AuthorizationRolesMap").GetSection(role).Get<string[]>()!
                );
    }

    private AuthorizationPolicyBuilder anonymousPolicy(AuthorizationPolicyBuilder authorizationPolicyBuilder)
    {
        authorizationPolicyBuilder.RequireAssertion(context =>
        {
            return true;
        });
        return authorizationPolicyBuilder;
    }
}
