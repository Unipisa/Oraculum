using OraculumApi.Models;
using WeaviateNET;

public class ChatDetailRepository : WeaviateRepository<ChatDetail>, IRepository<ChatDetail>
{
    public ChatDetailRepository(WeaviateDB db) : base(db)
    {
    }
}