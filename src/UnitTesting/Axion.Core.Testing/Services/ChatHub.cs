using Andux.Core.SignalR.Hubs;
using Andux.Core.SignalR.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Andux.Core.Testing.Services
{
    /// <summary>
    /// 示例聊天 Hub。
    /// </summary>
    public class ChatHub : BaseHub  // 或直接继承 Hub
    {
        public ChatHub(IUserConnectionManager userManager) : base(userManager) { }

        /// <summary>
        /// 客户端调用这个方法发消息给所有人。
        /// </summary>
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
