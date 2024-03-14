using Microsoft.AspNetCore.Authorization;
using OraculumApi.Models.BackOffice;

[Authorize(Policy = "sysadmin")]
public class MetricController : BaseController<Metric, MetricDTO>
{
    public MetricController(BaseService<Metric> service) : base(service)
    {
    }
}