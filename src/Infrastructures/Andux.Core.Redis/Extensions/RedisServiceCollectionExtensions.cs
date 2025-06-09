// =======================================
// 作者：andy.hu
// 文件：RedisServiceCollectionExtensions.cs
// 描述：redis 服务扩展
// =======================================

using Andux.Core.Redis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Andux.Core.Redis.Extensions
{
    /// <summary>
    /// 服务注册扩展方法
    /// </summary>
    public static class RedisServiceCollectionExtensions
    {
        /// <summary>
        /// 添加并配置 Redis 服务（基于 StackExchange.Redis）
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">应用配置，需包含 Redis 节点配置</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddRedisService(this IServiceCollection services, IConfiguration configuration)
        {
            // 绑定配置节到 RedisOptions
            services.Configure<AnduxRedisOptions>(configuration.GetSection("Redis"));

            // 读取 Redis 配置
            var redisOpts = configuration.GetSection("Redis").Get<AnduxRedisOptions>() ?? new AnduxRedisOptions();

            // 连接 Redis 服务器
            var muxer = ConnectionMultiplexer.Connect(redisOpts.Configuration);

            // 注册连接实例单例
            services.AddSingleton<IConnectionMultiplexer>(muxer);

            // 注册自定义 Redis 服务实现
            services.AddSingleton<IRedisService, RedisService>();

            return services;
        }

    }
}
