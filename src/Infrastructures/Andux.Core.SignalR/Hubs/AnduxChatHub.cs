using Microsoft.AspNetCore.SignalR;

namespace Andux.Core.SignalR.Hubs
{
    /// <summary>
    /// Andux Hub （内存）
    /// </summary>
    public class AnduxChatHub : BaseHub
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="userManager"></param>
        public AnduxChatHub(IUserConnectionManager userManager) 
            : base(userManager) { }

        /// <summary>
        /// 客户端调用这个方法发消息给所有人。
        /// </summary>
        public async Task SendMessage(string user, string message)
        {
            try
            {
                //Console.WriteLine($"SendMessage 被调用: {user} - {message}");
                await Clients.All.SendAsync("ReceiveMessage", user, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendMessage 内部异常: {ex.Message}\n{ex.StackTrace}");
                throw;  // 保留异常给 SignalR 上报
            }
        }
    }

    /// <summary>
    /// Andux Hub （redis）
    /// </summary>
    public class AnduxRedisChatHub : RedisBaseHub
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="redisUserManager"></param>
        public AnduxRedisChatHub(IRedisUserConnectionManager redisUserManager) 
            : base(redisUserManager) 
        { }

        /// <summary>
        /// 客户端调用这个方法发消息给所有人。
        /// </summary>
        public async Task SendMessage(string user, string message)
        {
            try
            {
                //Console.WriteLine($"SendMessage 被调用: {user} - {message}");
                await Clients.All.SendAsync("ReceiveMessage", user, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendMessage 内部异常: {ex.Message}\n{ex.StackTrace}");
                throw;  // 保留异常给 SignalR 上报
            }
        }
    }
}
