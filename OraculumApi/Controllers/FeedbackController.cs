using OraculumApi.Models.FrontOffice;

[DynamicAuthorize("sysadmin")]
public class FeedbackController : BaseController<Feedback, FeedbackDTO>
{
    public FeedbackController(BaseService<Feedback> service) : base(service)
    {
    }
}