using System;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class PresenceHub(PresenceTracker tracker) : Hub
{
    public async override Task OnConnectedAsync()
    {
        if(Context.User == null) throw new HubException("Cannot get current user claim");

        var isOnline = await tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);
        if(isOnline) await Clients.Others.SendAsync("UserIsOnline", Context.User?.GetUsername());

        var currentUsers = await tracker.GetOnlineUsers();
        await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
    }

    public async override Task OnDisconnectedAsync(Exception? exception)
    {
        if(Context.User == null) throw new HubException("Cannot get current user claim");

        var isOffline = await tracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);
        if(isOffline) await Clients.Others.SendAsync("UserIsOffline", Context.User?.GetUsername());

        // var currentUsers = await tracker.GetOnlineUsers();
        // await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
        
        await base.OnDisconnectedAsync(exception);
    }
}
