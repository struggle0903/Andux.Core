namespace Andux.Core.RabbitMQ.Interfaces
{
    /// <summary>
    /// RabbitMQ 消息发布接口
    /// </summary>
    public interface IRabbitMQPublisher
    {
        /// <summary>
        /// 将消息发布到指定队列。如果队列不存在，会自动创建持久化队列。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queueName">目标交换机名称</param>
        /// <param name="message">消息</param>
        /// <param name="persistent">是否持久化消息</param>
        void PublishToQueue<T>(string queueName, T message, bool persistent = true) where T : class;

        /// <summary>
        /// 将消息发布到指定交换机，并通过路由键路由到绑定队列。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exchangeName">目标交换机名称</param>
        /// <param name="routingKey">路由键</param>
        /// <param name="message">消息</param>
        /// <param name="persistent">是否持久化消息</param>
        void PublishToExchange<T>(string exchangeName, string routingKey, T message, bool persistent = true)
            where T : class;

        /// <summary>
        /// 批量发布消息到交换机，比单条发布有更高吞吐量。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exchangeName">目标交换机名称</param>
        /// <param name="messages">消息集合</param>
        /// <param name="persistent">是否持久化消息</param>
        void PublishBatch<T>(string exchangeName, IEnumerable<(string RoutingKey, T Message)> messages,
            bool persistent = true) where T : class;

    }
}
