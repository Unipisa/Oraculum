using Microsoft.AspNetCore.Authorization;

public static class AuthorizationAssertionRetriever
{
    public static AuthorizationPolicyBuilder retrieveAssertion(AuthorizationPolicyBuilder authorizationPolicyBuilder, string role, IConfiguration? securityConfiguration = null, bool AllowAnonymous = false)
    {

        if (AllowAnonymous)
        {
            return authorizationPolicyBuilder.RequireAssertion(context => { return true; });
        }

        if (securityConfiguration == null)
        {
            throw new ArgumentNullException(nameof(securityConfiguration));
        }

        var roleName = securityConfiguration.GetSection("Security").GetSection("AuthorizationRolesMap").GetSection(role).Get<string[]>();

        if (roleName == null)
        {
            throw new Exception($"Missing role {role} in a tentant's AuthorizationRolesMap.");
        }

        return authorizationPolicyBuilder.RequireAuthenticatedUser().RequireRole(roleName);
    }
}
