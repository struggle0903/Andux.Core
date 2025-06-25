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
        /// 添加并配置Andux SignalR核心服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="options">SignalR配置选项</param>
        /// <returns>配置后的服务集合</returns>
        /// <remarks>
        /// 1. 自动根据配置决定是否启用Redis作为背板
        /// 2. 注册核心Hub服务及用户连接管理服务
        /// 3. 提供Redis和非Redis两种实现方案
        /// </remarks>
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
