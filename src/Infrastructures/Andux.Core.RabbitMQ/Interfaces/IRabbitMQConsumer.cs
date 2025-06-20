namespace Andux.Core.RabbitMQ.Interfaces
{
    /// <summary>
    /// RabbitMQ 消息消费接口
    /// </summary>
    public interface IRabbitMQConsumer
    {
        /// <summary>
        /// 开始消费队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queueName"></param>
        /// <param name="handler"></param>
        /// <param name="autoAck"></param>
        void StartConsuming<T>(string queueName, Func<T, Task> handler, bool autoAck = false) where T : class;

        /// <summary>
        /// 开始消费指定租户的队列
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="tenantId">租户ID</param>
        /// <param name="queueName">队列名称</param>
        /// <param name="handler">消息处理委托</param>
        /// <param name="autoAck">是否自动确认</param>
        void StartConsuming<T>(string tenantId, string queueName, Func<T, Task> handler, bool autoAck = false) where T : class;

        /// <summary>
        /// 停止所有消费者
        /// </summary>
        void StopConsuming();

        /// <summary>
        /// 停止指定队列的消费者
        /// </summary>
        /// <param name="queueName"></param>
        void StopConsuming(string queueName);
    }
}
