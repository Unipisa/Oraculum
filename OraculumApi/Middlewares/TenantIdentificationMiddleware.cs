
public class TenantIdentificationMiddleware
{
    private readonly IConfiguration _configuration;
    private readonly RequestDelegate _next;

    public TenantIdentificationMiddleware(IConfiguration configuration, RequestDelegate next)
    {
        _configuration = configuration;
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var tenantId = context.Request.Host.Host;

        if(!_configuration.GetSection("Tenants").GetChildren().Any(x => x.Key == tenantId))
        {
            context.Items["TenantId"] = "Default";
            await _next(context);
            return;
        }
        
        context.Items["TenantId"] = tenantId;
        
        await _next(context);
    }
}
