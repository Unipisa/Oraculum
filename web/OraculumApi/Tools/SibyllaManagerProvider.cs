using Oraculum;

public class SibyllaManagerProvider
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _env;
    private readonly Dictionary<string, SibyllaManager> _managers = new();

    public SibyllaManagerProvider(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _env = env;
    }

    public SibyllaManager GetSibyllaManager()
    {
        var tenantId = _httpContextAccessor.HttpContext?.Items["TenantId"] as string;

        if (tenantId == null)
        {
            throw new Exception("TenantId not found in HttpContext.");
        }

        if (!_managers.ContainsKey(tenantId))
        {
            var sibyllaeConfigPath = Path.Combine(_env.ContentRootPath, "SibyllaeConf");
            var config = new Oraculum.OraculumConfiguration()
            {
                WeaviateEndpoint = _configuration[$"Tenants:{tenantId}:Weaviate:ServiceEndpoint"],
                WeaviateApiKey = _configuration[$"Tenants:{tenantId}:Weaviate:ApiKey"],
                ModelProvider = _configuration[$"Tenants:{tenantId}:GPTProvider"] == "Azure" ? OpenAI.ProviderType.Azure : OpenAI.ProviderType.OpenAi,
                OpenAIApiKey = _configuration[$"Tenants:{tenantId}:OpenAI:ApiKey"],
                OpenAIOrgId = _configuration[$"Tenants:{tenantId}:OpenAI:OrgId"],
                AzureOpenAIApiKey = _configuration[$"Tenants:{tenantId}:Azure:ApiKey"],
                AzureResourceName = _configuration[$"Tenants:{tenantId}:Azure:ResourceName"],
                AzureDeploymentId = _configuration[$"Tenants:{tenantId}:Azure:DeploymentId"],
                LocalModel =_configuration[$"Tenants:{tenantId}:EndPoint"]
            };

            if (
                string.IsNullOrEmpty(config.WeaviateEndpoint) || 
                config.ModelProvider == OpenAI.ProviderType.OpenAi && (string.IsNullOrEmpty(config.OpenAIApiKey) || string.IsNullOrEmpty(config.OpenAIOrgId)) ||
                config.ModelProvider == OpenAI.ProviderType.Azure && (string.IsNullOrEmpty(config.AzureOpenAIApiKey) || string.IsNullOrEmpty(config.AzureResourceName) || string.IsNullOrEmpty(config.AzureDeploymentId))
            )
            {
                throw new Exception("Sibylla is not properly configured for tenant " + tenantId);
            }

            _managers[tenantId] = new SibyllaManager(config, sibyllaeConfigPath);
        }

        return _managers[tenantId];
    }
}
