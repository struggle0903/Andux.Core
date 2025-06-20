namespace Andux.Core.RabbitMQ.Interfaces
{
    /// <summary>
    /// 租户专属RabbitMQ服务接口
    /// </summary>
    public interface IRabbitMQTenantService
    {
        /// <summary>
        /// 租户ID
        /// </summary>
        string TenantId { get; }

        /// <summary>
        /// 租户专属发布者
        /// </summary>
        IRabbitMQPublisher Publisher { get; }

        /// <summary>
        /// 租户专属消费者
        /// </summary>
        IRabbitMQConsumer Consumer { get; }
    }
}
