using Andux.Core.SignalR.Interfaces;
using Andux.Core.SignalR.Models;
using Microsoft.AspNetCore.SignalR;

namespace Andux.Core.SignalR.Hubs
{
    /// <summary>
    /// Hub 基类，封装统一连接管理。
    /// </summary>
    public abstract class BaseHub : Hub
    {
        private readonly IUserConnectionManager _userManager;

        protected BaseHub(IUserConnectionManager userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// 连接时注册用户。
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier ?? Context.ConnectionId;

            var user = new OnlineUserInfo
            {
                ConnectionId = Context.ConnectionId,
                UserId = userId,
                ConnectedAt = DateTime.UtcNow
            };

            _userManager.AddConnection(user);

            await Clients.Caller.SendAsync("OnConnected", user.ConnectionId);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 断开时移除用户。
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _userManager.RemoveConnection(Context.ConnectionId);
            await Clients.All.SendAsync("OnDisconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 客户端请求：获取所有在线用户。
        /// </summary>
        public async Task GetOnlineUsers()
        {
            var users = _userManager.GetAllConnections();
            await Clients.Caller.SendAsync("OnlineUserList", users);
        }
    }
}
