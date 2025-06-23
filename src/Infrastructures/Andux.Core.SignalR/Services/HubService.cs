using Andux.Core.SignalR.Hubs;
using Andux.Core.SignalR.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Andux.Core.SignalR.Services
{
    /// <summary>
    /// SignalR 消息推送业务实现。
    /// </summary>
    public class HubService : IHubService
    {
        private readonly IHubContext<BaseHub> _hubContext;

        public HubService(IHubContext<BaseHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task BroadcastAsync(string user, string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendToConnectionAsync(string connectionId, string message)
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", "系统", message);
        }

        public async Task SendToGroupAsync(string groupName, string message)
        {
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", "系统", message);
        }

        public async Task RemoveUserConnectionAsync(string connectionId)
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("ForceDisconnect");
        }

        public Task SendToUserAsync(string connectionId, string message)
        {
            throw new NotImplementedException();
        }

    }
}
