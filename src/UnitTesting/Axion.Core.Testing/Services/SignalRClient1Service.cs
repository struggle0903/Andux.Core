using Andux.Core.SignalR.Clients;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Andux.Core.Testing.Services
{
    /// <summary>
    /// SignalR测试客户端1
    /// </summary>
    public class SignalRClient1Service : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<OrderProcessingService> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="services"></param>
        /// <param name="logger"></param>
        public SignalRClient1Service(
            IServiceProvider services,
            ILogger<OrderProcessingService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SignalR测试客户端【1】正在启动.");

            // 10 秒后异步执行，不阻塞当前主线程
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000, stoppingToken); // 等待5秒
                if (stoppingToken.IsCancellationRequested) return;

                var client = new SignalRClient("http://192.168.1.88:5001/chatHub");
                client.On<string, string>("ReceiveMessage", (u, m) => 
                {
                    Console.WriteLine($"户端【1】收到: {u} - {m}");
                });

                await client.ConnectAsync();

                client.On<string>("OnConnected", (connId) =>
                {
                    Console.WriteLine($"户端【1】连接成功，连接ID: {connId}");
                });

                try
                {
                    await client.SendMessageAsync("SendMessage", "我是户端【1】", "你好 SignalR");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"调用 SendMessage 出错: {ex.Message}");
                }

            }, stoppingToken);

        }
    }
}
