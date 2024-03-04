using OraculumApi.Models;

[DynamicAuthorize("sysadmin")]
public class ChatDetailController : BaseController<ChatDetail, ChatDetailDTO>
{
    public ChatDetailController(ChatDetailService service) : base(service)
    {
    }
}