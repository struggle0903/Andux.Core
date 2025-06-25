namespace Andux.Core.EventBus.Events
{
    /// <summary>
    /// RabbitMQ模式事件总线接口
    /// </summary>
    public interface IRabbitMQEventBus : IEventBus
    {
        /// <summary>
        /// 发布-显式指定队列名
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="event"></param>
        /// <param name="queueName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task PublishAsync<TEvent>(TEvent @event, string queueName, CancellationToken cancellationToken = default)
            where TEvent : IEvent;

        /// <summary>
        /// 订阅-显式指定队列名
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="THandler"></typeparam>
        /// <param name="queueName"></param>
        /// <returns></returns>
        Task SubscribeAsync<TEvent, THandler>(string queueName)
            where TEvent : IEvent
            where THandler : IEventHandler<TEvent>;
    }
}
