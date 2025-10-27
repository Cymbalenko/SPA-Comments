using Microsoft.AspNetCore.SignalR;

namespace SPA_Comments.Server.Hubs;

public class CommentHub : Hub, ICommentHub
{
    public async Task SendComment(object comment)
    { 
        await Clients.All.SendAsync("ReceiveComment", comment);
    }
}
