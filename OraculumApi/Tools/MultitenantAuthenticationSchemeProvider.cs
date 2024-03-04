using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

public class MultitenantAuthenticationSchemeProvider : AuthenticationSchemeProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MultitenantAuthenticationSchemeProvider(IOptions<AuthenticationOptions> options, IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<AuthenticationScheme?> GetDefaultAuthenticateSchemeAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null && context.Items.ContainsKey("TenantId"))
        {
            var tenantId = context.Items["TenantId"] as string;
            if (!string.IsNullOrEmpty(tenantId))
            {
                return await GetSchemeAsync(tenantId);
            }
        }

        return await base.GetDefaultAuthenticateSchemeAsync();
    }
}
