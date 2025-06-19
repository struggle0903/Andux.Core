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
        /// 停止消费
        /// </summary>
        void StopConsuming();

        /// <summary>
        /// 停止指定消费者
        /// </summary>
        /// <param name="queueName"></param>
        void StopConsuming(string queueName);

    }
}
