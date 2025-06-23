using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;

namespace Andux.Core.SignalR.Middlewares
{
    /// <summary>
    /// SignalR 日志中间件，可记录连接/断开/调用日志。
    /// </summary>
    public class HubLoggerMiddleware : IHubFilter
    {
        private readonly ILogger<HubLoggerMiddleware> _logger;

        public HubLoggerMiddleware(ILogger<HubLoggerMiddleware> logger)
        {
            _logger = logger;
        }

        public async ValueTask<object?> InvokeMethodAsync(HubInvocationContext context, Func<HubInvocationContext, ValueTask<object?>> next)
        {
            _logger.LogInformation("调用 Hub 方法：{Method}，参数：{Args}", context.HubMethodName, context.HubMethodArguments);
            return await next(context);
        }

        public async Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
        {
            _logger.LogInformation("连接建立：{ConnectionId}", context.Context.ConnectionId);
            await next(context);
        }

        public async Task OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, Task> next)
        {
            _logger.LogInformation("连接断开：{ConnectionId}", context.Context.ConnectionId);
            await next(context, exception);
        }
    }
}
