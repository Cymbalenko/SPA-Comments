using HotChocolate.AspNetCore.Subscriptions.Protocols;

namespace SPA_Comments.Server.Hubs;

public interface ICommentHub
{
    public Task SendComment(object comment);
}
