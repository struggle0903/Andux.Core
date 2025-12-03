using Andux.Core.RabbitMQ.Interfaces;
using RabbitMQ.Client;

namespace Andux.Core.RabbitMQ.Services.Tenant
{
    /// <summary>
    /// 租户专属RabbitMQ服务实现
    /// </summary>
    public class RabbitMQTenantService : IRabbitMQTenantService
    {
        public string TenantId { get; }
        public bool EnablePrefix { get; }
        public DateTime CreatedTime { get; }
        public IRabbitMQPublisher Publisher { get; }
        public IRabbitMQConsumer Consumer { get; }

        public RabbitMQTenantService(
            string? tenantId,
            bool enablePrefix,
            IRabbitMQConnectionProvider connectionProvider,
            IRabbitMQPublisher publisher,
            IRabbitMQConsumer consumer,
            IRabbitMQTenantServiceFactory tenantServiceFactory)
        {
            TenantId = tenantId ?? string.Empty;
            EnablePrefix = enablePrefix;
            CreatedTime = DateTime.Now;
            Publisher = new TenantPublisherDecorator(connectionProvider, publisher, tenantId, enablePrefix);
            Consumer = new TenantConsumerDecorator(connectionProvider, consumer, tenantId, enablePrefix);

            // 确保租户已注册
            //connectionProvider.GetTenantConnection(tenantId);
        }

        /// <summary>
        /// 租户发布者
        /// </summary>
        private class TenantPublisherDecorator : IRabbitMQPublisher
        {
            private readonly IRabbitMQPublisher _inner;
            private readonly IRabbitMQConnectionProvider _connectionProvider;
            private readonly string _tenantPrefix;
            private readonly string _currentTenantId;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="connectionProvider"></param>
            /// <param name="inner"></param>
            /// <param name="tenantId"></param>
            /// <param name="enablePrefix"></param>
            public TenantPublisherDecorator(
                IRabbitMQConnectionProvider connectionProvider,
                IRabbitMQPublisher inner, 
                string? tenantId,
                bool enablePrefix)
            {
                _inner = inner;
                _currentTenantId = tenantId ?? "default";
                _connectionProvider = connectionProvider;
                _tenantPrefix = enablePrefix ? (tenantId is { Length: > 0 } ? $"{tenantId}." : ""): "";
            }

            private string GetTenantName(string name) => $"{_tenantPrefix}{name}";

            /// <summary>
            /// 将消息发布到指定队列。如果队列不存在，会自动创建持久化队列。
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="queueName">目标交换机名称</param>
            /// <param name="message">消息</param>
            /// <param name="persistent">是否持久化消息</param>
            public void PublishToQueue<T>(string queueName, T message, bool persistent = true) where T : class
            {
                var tenantConnection = _connectionProvider.GetTenantConnection(_currentTenantId);
                _inner.PublishToQueue(tenantConnection, GetTenantName(queueName), message, persistent);
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
                var tenantConnection = _connectionProvider.GetTenantConnection(_currentTenantId);
                _inner.PublishToExchange(tenantConnection, GetTenantName(exchangeName), GetTenantName(routingKey), message, persistent);
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
                var modifiedMessages = messages.Select(x => (GetTenantName(x.RoutingKey), x.Message));
                var tenantConnection = _connectionProvider.GetTenantConnection(_currentTenantId);
                _inner.PublishBatch(tenantConnection, GetTenantName(exchangeName), modifiedMessages, persistent);
            }

            /// <summary>
            /// 将消息发布到指定交换机，并通过路由键路由到绑定队列。
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="exchangeName">目标交换机名称</param>
            /// <param name="routingKey">路由键</param>
            /// <param name="message">消息</param>
            /// <param name="persistent">是否持久化消息</param>
            public void PublishTopic<T>(string exchangeName, string routingKey, T message, bool persistent = true) where T : class
            {
                var tenantConnection = _connectionProvider.GetTenantConnection(_currentTenantId);
                _inner.PublishTopic(tenantConnection, GetTenantName(exchangeName), GetTenantName(routingKey), message, persistent);
            }

            #region 根据Connection连接对象发布消息

            /// <summary>
            /// 将消息发布到指定队列。如果队列不存在，会自动创建持久化队列。
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="connection">连接对象</param>
            /// <param name="queueName">目标交换机名称</param>
            /// <param name="message">消息</param>
            /// <param name="persistent">是否持久化消息</param>
            public void PublishToQueue<T>(IConnection connection, string queueName, T message, bool persistent = true) where T : class
            {
                _inner.PublishToQueue(connection, GetTenantName(queueName), message, persistent);
            }

            /// <summary>
            /// 将消息发布到指定交换机，并通过路由键路由到绑定队列。
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="connection">连接对象</param>
            /// <param name="exchangeName">目标交换机名称</param>
            /// <param name="routingKey">路由键</param>
            /// <param name="message">消息</param>
            /// <param name="persistent">是否持久化消息</param>
            public void PublishToExchange<T>(IConnection connection, string exchangeName, string routingKey, T message, bool persistent = true) where T : class
            {
                _inner.PublishToExchange(connection, GetTenantName(exchangeName), GetTenantName(routingKey), message, persistent);
            }

            /// <summary>
            /// 批量发布消息到交换机，比单条发布有更高吞吐量。
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="connection">连接对象</param>
            /// <param name="exchangeName">目标交换机名称</param>
            /// <param name="messages">消息集合</param>
            /// <param name="persistent">是否持久化消息</param>
            public void PublishBatch<T>(IConnection connection, string exchangeName, IEnumerable<(string RoutingKey, T Message)> messages, bool persistent = true) where T : class
            {
                var modifiedMessages = messages.Select(x => (GetTenantName(x.RoutingKey), x.Message));
                _inner.PublishBatch(connection, GetTenantName(exchangeName), modifiedMessages, persistent);
            }

            /// <summary>
            /// 将消息发布到指定交换机，并通过路由键路由到绑定队列。
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="connection">连接对象</param>
            /// <param name="exchangeName">目标交换机名称</param>
            /// <param name="routingKey">路由键</param>
            /// <param name="message">消息</param>
            /// <param name="persistent">是否持久化消息</param>
            public void PublishTopic<T>(IConnection connection, string exchangeName, string routingKey, T message, bool persistent = true) where T : class
            {
                _inner.PublishTopic(connection, GetTenantName(exchangeName), GetTenantName(routingKey), message, persistent);
            }

            #endregion
        }

        /// <summary>
        /// 租户消费者
        /// </summary>
        private class TenantConsumerDecorator : IRabbitMQConsumer
        {
            private readonly IRabbitMQConsumer _inner;
            private readonly IRabbitMQConnectionProvider _connectionProvider;
            private readonly string _tenantPrefix;
            private readonly string _currentTenantId;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="inner"></param>
            /// <param name="connectionProvider"></param>
            /// <param name="tenantId"></param>
            /// <param name="enablePrefix"></param>
            public TenantConsumerDecorator(
                IRabbitMQConnectionProvider connectionProvider, 
                IRabbitMQConsumer inner,
                string? tenantId,
                bool enablePrefix)
            {
                _inner = inner;
                _currentTenantId = tenantId ?? "default";
                _connectionProvider = connectionProvider;
                _tenantPrefix = enablePrefix ? (tenantId is { Length: > 0 } ? $"{tenantId}." : "") : "";
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
                var tenantChannel = _connectionProvider.CreateChannel(_currentTenantId);
                _inner.StartConsuming(tenantChannel, GetTenantName(queueName), handler, autoAck);
            }

            /// <summary>
            /// 订阅 Topic Exchange 消息
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="exchangeName">Topic 交换机名称</param>
            /// <param name="routingKey">路由键，可使用 * 或 # 通配符</param>
            /// <param name="handler">消息处理方法</param>
            /// <param name="autoAck">是否自动确认</param>
            public void StartConsumingTopic<T>(string exchangeName, string routingKey, Func<T, Task> handler, bool autoAck = false) where T : class
            {
                var tenantChannel = _connectionProvider.CreateChannel(_currentTenantId);
                _inner.StartConsumingTopic(tenantChannel, GetTenantName(exchangeName), GetTenantName(routingKey), handler, autoAck);
            }

            /// <summary>
            /// 订阅 Topic Exchange 消息（自定义队列名）
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="exchangeName">Topic 交换机名称</param>
            /// <param name="routingKey">路由键，可使用 * 或 # 通配符</param>
            /// <param name="queueName">用户订阅队列名</param>
            /// <param name="handler">消息处理方法</param>
            /// <param name="autoAck">是否自动确认</param>
            public void StartConsumingTopic<T>(string exchangeName, string routingKey, string queueName,
                Func<T, Task> handler, bool autoAck = false) where T : class
            {
                var tenantChannel = _connectionProvider.CreateChannel(_currentTenantId);
                _inner.StartConsumingTopic(tenantChannel, GetTenantName(exchangeName), GetTenantName(routingKey), GetTenantName(queueName), handler, autoAck);
            }

            /// <summary>
            /// 开始消费 Exchange 消息（支持 Topic 或 Fanout）
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="exchangeName">Exchange 名称</param>
            /// <param name="queueName">可选：固定队列名。如果为 null 或空，则创建临时随机队列，用于广播模式</param>
            /// <param name="routingKey">路由键，可使用 * 或 # 通配符，Fanout 可为空</param>
            /// <param name="handler">消息处理方法</param>
            /// <param name="autoAck">是否自动确认</param>
            public void StartConsumingExchange<T>(
                string exchangeName,
                string? queueName,
                string routingKey,
                Func<T, Task> handler,
                bool autoAck = false) where T : class
            {
                var tenantChannel = _connectionProvider.CreateChannel(_currentTenantId);
                _inner.StartConsumingExchange(tenantChannel, GetTenantName(exchangeName), GetTenantName(queueName ?? "default"), GetTenantName(routingKey), handler, autoAck);
            }

            #region 根据指定通道订阅消息
            public void StartConsuming<T>(IModel channel, string queueName, Func<T, Task> handler, bool autoAck = false) where T : class
            {
                _inner.StartConsuming(channel, GetTenantName(queueName), handler, autoAck);
            }

            public void StartConsumingTopic<T>(IModel channel, string exchangeName, string routingKey, Func<T, Task> handler, bool autoAck = false) where T : class
            {
                _inner.StartConsumingTopic(channel, GetTenantName(exchangeName), GetTenantName(routingKey), handler, autoAck);
            }

            public void StartConsumingTopic<T>(IModel channel, string exchangeName, string routingKey, string queueName, Func<T, Task> handler, bool autoAck = false) where T : class
            {
                _inner.StartConsumingTopic(channel, GetTenantName(exchangeName), GetTenantName(routingKey), GetTenantName(queueName), handler, autoAck);
            }

            public void StartConsumingExchange<T>(IModel channel, string exchangeName, string? queueName, string routingKey, Func<T, Task> handler, bool autoAck = false) where T : class
            {
                _inner.StartConsumingExchange(channel, GetTenantName(exchangeName), GetTenantName(queueName ?? "default"), GetTenantName(routingKey), handler, autoAck);
            }
            #endregion

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
