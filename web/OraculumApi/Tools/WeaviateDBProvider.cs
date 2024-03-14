using WeaviateNET;

public class WeaviateDBProvider
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Dictionary<string, WeaviateDB> _databases = new();

    public WeaviateDBProvider(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public WeaviateDB GetDatabase()
    {
        var tenantId = _httpContextAccessor.HttpContext?.Items["TenantId"] as string;

        if (tenantId == null)
        {
            throw new Exception("TenantId not found in HttpContext.");
        }

        if (!_databases.ContainsKey(tenantId))
        {
            var endpoint = _configuration[$"Tenants:{tenantId}:Weaviate:ServiceEndpoint"];
            var apiKey = _configuration[$"Tenants:{tenantId}:Weaviate:ApiKey"];

            if (string.IsNullOrEmpty(endpoint))
            {
                throw new Exception("Weaviate endpoint not found for tenant " + tenantId);
            }

            _databases[tenantId] = new WeaviateDB(endpoint, apiKey);
        }

        return _databases[tenantId];
    }
}
