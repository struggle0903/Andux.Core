using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Andux.Core.EventBus.Core;
using Andux.Core.EventBus.Events;

namespace Andux.Core.EventBus.Extensions
{
    /// <summary>
    /// EventBus 相关的 IServiceCollection 扩展方法，方便在 Startup 或 Program 中注册事件总线服务。
    /// </summary>
    public static class EventBusServiceCollectionExtensions
    {
        /// <summary>
        /// 根据配置文件中 EventBus 节点配置，注册对应的事件总线实现。
        /// </summary>
        /// <param name="services">依赖注入服务集合</param>
        /// <param name="configuration">应用程序配置对象</param>
        /// <returns>返回同一个 IServiceCollection，支持链式调用</returns>
        public static IServiceCollection UseAnduxEventBus(this IServiceCollection services, IConfiguration configuration)
        {
            // 将 "EventBus" 节点的配置绑定到 EventBusOptions 类，供后续注入使用
            services.Configure<AnduxEventBusOptions>(configuration.GetSection("EventBus"));

            // 从配置中读取 EventBusOptions 对象，如果不存在则抛出异常
            var options = configuration.GetSection("EventBus").Get<AnduxEventBusOptions>()
                          ?? throw new InvalidOperationException("缺少 EventBus 配置");

            // 根据配置中 Provider 字段选择注入不同的事件总线实现
            return options.Provider switch
            {
                "InMemory" => services.AddSingleton<IEventBus, InMemoryEventBus>(),  // 内存事件总线
                "RabbitMQ" => services.AddSingleton<IEventBus, RabbitMqEventBus>(),  // RabbitMQ 事件总线
                _ => throw new InvalidOperationException("Unknown provider: " + options.Provider)  // 未知提供者报错
            };
        }
    }
}
