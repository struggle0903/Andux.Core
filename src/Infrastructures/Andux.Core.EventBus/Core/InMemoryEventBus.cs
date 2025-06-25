using Andux.Core.EventBus.Events;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Andux.Core.EventBus.Core
{
    /// <summary>
    /// 基于内存的事件总线实现，适合单机或测试环境，支持订阅和发布事件。
    /// </summary>
    public class InMemoryEventBus : IEventBus
    {
        /// <summary>
        /// 用于创建事件处理器实例的服务提供者，支持依赖注入作用域。
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 存储事件类型到处理器类型列表的映射，线程安全。
        /// key：事件类型，value：处理该事件的处理器类型列表
        /// </summary>
        private readonly ConcurrentDictionary<Type, List<Type>> _handlers = new();

        /// <summary>
        /// 构造函数，注入服务提供者。
        /// </summary>
        /// <param name="serviceProvider">依赖注入服务提供者</param>
        public InMemoryEventBus(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 订阅事件，将指定事件类型和处理器类型关联起来。
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <typeparam name="THandler">事件处理器类型，必须实现 IEventHandler&lt;TEvent&gt;</typeparam>
        public Task SubscribeAsync<TEvent, THandler>()
            where TEvent : IEvent
            where THandler : IEventHandler<TEvent>
        {
            var eventType = typeof(TEvent);
            var handlerType = typeof(THandler);

            // 使用线程安全的 AddOrUpdate 保证多线程下订阅安全
            _handlers.AddOrUpdate(eventType,
                // 如果不存在则创建新的处理器列表
                _ => new List<Type> { handlerType },
                // 如果已存在，判断是否已包含处理器，避免重复添加
                (_, existing) =>
                {
                    if (!existing.Contains(handlerType))
                        existing.Add(handlerType);
                    return existing;
                });

            return Task.CompletedTask;
        }

        /// <summary>
        /// 发布事件，依次调用所有订阅该事件的处理器异步处理。
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="event">事件实例</param>
        /// <param name="cancellationToken">取消令牌</param>
        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : IEvent
        {
            if (_handlers.TryGetValue(typeof(TEvent), out var handlerTypes))
            {
                // 创建 DI 作用域，确保事件处理器的生命周期正确
                using var scope = _serviceProvider.CreateScope();

                // 依次解析每个处理器实例并调用 HandleAsync
                foreach (var handlerType in handlerTypes)
                {
                    var handler = (IEventHandler<TEvent>)scope.ServiceProvider.GetRequiredService(handlerType);
                    await handler.HandleAsync(@event, cancellationToken);
                }
            }
        }

    }
}
