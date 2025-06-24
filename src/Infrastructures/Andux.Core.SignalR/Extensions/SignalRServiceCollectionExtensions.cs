using Andux.Core.SignalR.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Andux.Core.SignalR.Extensions
{
    /// <summary>
    /// SignalR 服务集成扩展。
    /// </summary>
    public static class SignalRServiceCollectionExtensions
    {
        /// <summary>
        /// 使用AnduxSignalR 服务
        /// </summary>
        public static IServiceCollection UseAnduxSignalR(this IServiceCollection services, SignalROptions options)
        {
            var builder = services.AddSignalR();

            if (!string.IsNullOrWhiteSpace(options.RedisConnection))
            {
                builder.AddStackExchangeRedis(options.RedisConnection);
            }

            services.AddSingleton<IHubService, HubService>();
            services.AddSingleton<IUserConnectionManager, UserConnectionManager>();
            services.AddSingleton<IRedisHubService, RedisHubService>();
            services.AddSingleton<IRedisUserConnectionManager, RedisUserConnectionManager>();

            return services;
        }

    }
}
