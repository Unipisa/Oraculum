using OraculumApi.Models;

[DynamicAuthorize("sysadmin")]
public class SibyllaPersistentConfigController : BaseController<SibyllaPersistentConfig, SibyllaPersistentConfigDTO>
{
    public SibyllaPersistentConfigController(BaseService<SibyllaPersistentConfig> service) : base(service)
    {
    }
}