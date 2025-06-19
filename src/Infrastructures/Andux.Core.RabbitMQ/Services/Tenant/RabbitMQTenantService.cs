using Andux.Core.RabbitMQ.Interfaces;

namespace Andux.Core.RabbitMQ.Services.Tenant
{
    /// <summary>
    /// 租户专属RabbitMQ服务实现
    /// </summary>
    public class RabbitMQTenantService : IRabbitMQTenantService
    {
        public string TenantId { get; }
        public IRabbitMQPublisher Publisher { get; }
        public IRabbitMQConsumer Consumer { get; }

        public RabbitMQTenantService(
            string tenantId,
            IRabbitMQConnectionProvider connectionProvider,
            IRabbitMQPublisher publisher,
            IRabbitMQConsumer consumer)
        {
            TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
            Publisher = new TenantPublisherDecorator(publisher, tenantId);
            Consumer = new TenantConsumerDecorator(consumer, tenantId);

            // 确保租户已注册
            connectionProvider.GetTenantConnection(tenantId);
        }

        /// <summary>
        /// 租户发布者
        /// </summary>
        private class TenantPublisherDecorator : IRabbitMQPublisher
        {
            private readonly IRabbitMQPublisher _inner;
            private readonly string _tenantPrefix;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="inner"></param>
            public TenantPublisherDecorator(IRabbitMQPublisher inner, string tenantId)
            {
                _inner = inner;
                _tenantPrefix = $"{tenantId}_";
            }

            /// <summary>
            /// 将消息发布到指定队列。如果队列不存在，会自动创建持久化队列。
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="queueName">目标交换机名称</param>
            /// <param name="message">消息</param>
            /// <param name="persistent">是否持久化消息</param>
            public void PublishToQueue<T>(string queueName, T message, bool persistent = true) where T : class
            {
                _inner.PublishToQueue(GetTenantName(queueName), message, persistent);
            }

            /// <summary>
            /// 将消息发布到指定交换机，并通过路由键路由到绑定队列。
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="exchangeName">目标交换机名称</param>
            /// <param name="routingKey">路由键</param>
            /// <param name="message">消息</param>
            /// <param name="persistent">是否持久化消息</param>
            public void PublishToExchange<T>(string exchangeName, string routingKey, T message, bool persistent = true) where T : class
            {
                _inner.PublishToExchange(GetTenantName(exchangeName), routingKey, message, persistent);
            }

            /// <summary>
            /// 批量发布消息到交换机，比单条发布有更高吞吐量。
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="exchangeName">目标交换机名称</param>
            /// <param name="messages">消息集合</param>
            /// <param name="persistent">是否持久化消息</param>
            public void PublishBatch<T>(string exchangeName, IEnumerable<(string RoutingKey, T Message)> messages, bool persistent = true) where T : class
            {
                _inner.PublishBatch(GetTenantName(exchangeName), messages, persistent);
            }

            private string GetTenantName(string name) => $"{_tenantPrefix}{name}";
        }

        /// <summary>
        /// 租户消费者
        /// </summary>
        private class TenantConsumerDecorator : IRabbitMQConsumer
        {
            private readonly IRabbitMQConsumer _inner;
            private readonly string _tenantPrefix;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="inner"></param>
            public TenantConsumerDecorator(IRabbitMQConsumer inner, string tenantId)
            {
                _inner = inner;
                _tenantPrefix = $"{tenantId}_";
            }

            /// <summary>
            /// 开始消费队列
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="queueName"></param>
            /// <param name="handler"></param>
            /// <param name="autoAck"></param>
            public void StartConsuming<T>(string queueName, Func<T, Task> handler, bool autoAck = false) where T : class
            {
                _inner.StartConsuming(GetTenantName(queueName), handler, autoAck);
            }

            /// <summary>
            /// 停止消费
            /// </summary>
            public void StopConsuming()
            {
                _inner.StopConsuming();
            }

            /// <summary>
            /// 停止指定消费者
            /// </summary>
            /// <param name="queueName"></param>
            public void StopConsuming(string queueName)
            {
                _inner.StopConsuming(GetTenantName(queueName));
            }

            private string GetTenantName(string name) => $"{_tenantPrefix}{name}";
        }
    }
}
