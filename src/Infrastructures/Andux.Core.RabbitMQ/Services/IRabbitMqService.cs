namespace Andux.Core.RabbitMQ.Services
{
    /// <summary>
    /// RabbitMQ 管理接口。
    /// </summary>
    public interface IRabbitMqService
    {
        /// <summary>
        /// 发布消息
        /// </summary>
        Task PublishAsync(string userName, string exchange, string routingKey, string message);

        /// <summary>
        /// 订阅消息
        /// </summary>
        void Subscribe(string userName, string queue, string exchange, string routingKey, Action<string> onMessageReceived);
    }
}
