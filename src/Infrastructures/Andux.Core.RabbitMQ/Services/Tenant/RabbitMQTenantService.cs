using Andux.Core.RabbitMQ.Interfaces;
using Andux.Core.RabbitMQ.Models;
using RabbitMQ.Client;

namespace Andux.Core.RabbitMQ.Services.Tenant
{
    /// <summary>
    /// 租户专属RabbitMQ服务实现
    /// </summary>
    public class RabbitMQTenantService : IRabbitMQTenantService
    {
        public string TenantId { get; }
        public RabbitMQTenantOptions TenantOptions { get; }
        public DateTime CreatedTime { get; }
        public IRabbitMQPublisher Publisher { get; }
        public IRabbitMQConsumer Consumer { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="mqTenantOptions"></param>
        /// <param name="connectionProvider"></param>
        /// <param name="publisher"></param>
        /// <param name="consumer"></param>
        public RabbitMQTenantService(
            string? tenantId,
            RabbitMQTenantOptions mqTenantOptions,
            IRabbitMQConnectionProvider connectionProvider,
            IRabbitMQPublisher publisher,
            IRabbitMQConsumer consumer)
        {
            TenantId = tenantId ?? string.Empty;
            CreatedTime = DateTime.Now;
            TenantOptions = mqTenantOptions;
            Publisher = new TenantPublisherDecorator(connectionProvider, publisher, mqTenantOptions, tenantId);
            Consumer = new TenantConsumerDecorator(connectionProvider, consumer, mqTenantOptions, tenantId);

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
            private readonly string _exchangePrefix;
            private readonly string _routingKeyPrefix;
            private readonly string _queuePrefix;
            private readonly string _currentTenantId;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="connectionProvider"></param>
            /// <param name="inner"></param>
            /// <param name="tenantOptions"></param>
            /// <param name="tenantId"></param>
            public TenantPublisherDecorator(
                IRabbitMQConnectionProvider connectionProvider,
                IRabbitMQPublisher inner,
                RabbitMQTenantOptions tenantOptions,
                string? tenantId)
            {
                _inner = inner;
                _currentTenantId = tenantId ?? "default";
                _connectionProvider = connectionProvider;
                _exchangePrefix = tenantOptions.EnableExchangePrefix ? (tenantId is { Length: > 0 } ? $"{tenantId}." : ""): "";
                _routingKeyPrefix = tenantOptions.EnableRoutingKeyPrefix ? (tenantId is { Length: > 0 } ? $"{tenantId}." : ""): "";
                _queuePrefix = tenantOptions.EnableQueueNamePrefix ? (tenantId is { Length: > 0 } ? $"{tenantId}." : ""): "";
            }

            /// <summary>
            /// 租户交换机名称
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            private string GetTenantExchangeName(string name) => $"{_exchangePrefix}{name}";

            /// <summary>
            /// 租户路由key名称
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            private string GetTenantRoutingKeyName(string name) => $"{_routingKeyPrefix}{name}";

            /// <summary>
            /// 租户队列名称
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            private string GetTenantQueueName(string name) => $"{_queuePrefix}{name}";

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
                _inner.PublishToQueue(tenantConnection, GetTenantQueueName(queueName), message, persistent);
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
                _inner.PublishToExchange(tenantConnection, GetTenantExchangeName(exchangeName), GetTenantRoutingKeyName(routingKey), message, persistent);
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
                var modifiedMessages = messages.Select(x => (GetTenantRoutingKeyName(x.RoutingKey), x.Message));
                var tenantConnection = _connectionProvider.GetTenantConnection(_currentTenantId);
                _inner.PublishBatch(tenantConnection, GetTenantExchangeName(exchangeName), modifiedMessages, persistent);
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
                _inner.PublishTopic(tenantConnection, GetTenantExchangeName(exchangeName), GetTenantRoutingKeyName(routingKey), message, persistent);
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
                _inner.PublishToQueue(connection, GetTenantQueueName(queueName), message, persistent);
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
                _inner.PublishToExchange(connection, GetTenantExchangeName(exchangeName), GetTenantRoutingKeyName(routingKey), message, persistent);
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
                var modifiedMessages = messages.Select(x => (GetTenantRoutingKeyName(x.RoutingKey), x.Message));
                _inner.PublishBatch(connection, GetTenantExchangeName(exchangeName), modifiedMessages, persistent);
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
                _inner.PublishTopic(connection, GetTenantExchangeName(exchangeName), GetTenantRoutingKeyName(routingKey), message, persistent);
            }

            /// <summary>
            /// 发布广播消息（Fanout 交换机，所有绑定队列都会收到）
            /// </summary>
            /// <typeparam name="T">消息类型</typeparam>
            /// <param name="exchangeName">目标交换机名称</param>
            /// <param name="message">要广播的消息</param>
            /// <param name="persistent">是否持久化消息</param>
            public void PublishBroadcast<T>(string exchangeName, T message, bool persistent = true) where T : class
            {
                _inner.PublishBroadcast(GetTenantExchangeName(exchangeName), message, persistent);
            }

            /// <summary>
            /// 发布广播消息到临时广播交换机
            /// </summary>
            /// <typeparam name="T">消息类型</typeparam>
            /// <param name="exchangeName">交换机名称</param>
            /// <param name="message">要广播的消息</param>
            /// <param name="autoDelete">是否自动删除（临时广播设为true）</param>
            /// <remarks>
            /// 适用于临时广播场景，如实时通知、会话消息等
            /// 连接断开后交换机会自动删除
            /// </remarks>
            public void PublishTemporaryBroadcast<T>(string exchangeName, T message, bool autoDelete = true) where T : class
            {
                _inner.PublishTemporaryBroadcast(GetTenantExchangeName(exchangeName), message, autoDelete);
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
            private readonly string _exchangePrefix;
            private readonly string _routingKeyPrefix;
            private readonly string _queuePrefix;
            private readonly string _currentTenantId;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="inner"></param>
            /// <param name="connectionProvider"></param>
            /// <param name="tenantOptions"></param>
            /// <param name="tenantId"></param>
            public TenantConsumerDecorator(
                IRabbitMQConnectionProvider connectionProvider, 
                IRabbitMQConsumer inner,
                RabbitMQTenantOptions tenantOptions,
                string? tenantId)
            {
                _inner = inner;
                _currentTenantId = tenantId ?? "default";
                _connectionProvider = connectionProvider;
                _exchangePrefix = tenantOptions.EnableExchangePrefix ? (tenantId is { Length: > 0 } ? $"{tenantId}." : "") : "";
                _routingKeyPrefix = tenantOptions.EnableRoutingKeyPrefix ? (tenantId is { Length: > 0 } ? $"{tenantId}." : "") : "";
                _queuePrefix = tenantOptions.EnableQueueNamePrefix ? (tenantId is { Length: > 0 } ? $"{tenantId}." : "") : "";
            }

            /// <summary>
            /// 租户交换机名称
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            private string GetTenantExchangeName(string name) => $"{_exchangePrefix}{name}";

            /// <summary>
            /// 租户路由key名称
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            private string GetTenantRoutingKeyName(string name) => $"{_routingKeyPrefix}{name}";

            /// <summary>
            /// 租户队列名称
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            private string GetTenantQueueName(string name) => $"{_queuePrefix}{name}";

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
                _inner.StartConsuming(tenantChannel, GetTenantQueueName(queueName), handler, autoAck);
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
                _inner.StartConsumingTopic(tenantChannel, GetTenantExchangeName(exchangeName), GetTenantRoutingKeyName(routingKey), handler, autoAck);
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
                _inner.StartConsumingTopic(tenantChannel, GetTenantExchangeName(exchangeName), GetTenantRoutingKeyName(routingKey), GetTenantQueueName(queueName), handler, autoAck);
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
                _inner.StartConsumingExchange(tenantChannel, GetTenantExchangeName(exchangeName), GetTenantQueueName(queueName ?? "default"), GetTenantRoutingKeyName(routingKey), handler, autoAck);
            }

            /// <summary>
            /// 开始消费广播消息（Fanout 交换机专用）
            /// </summary>
            /// <typeparam name="T">消息类型</typeparam>
            /// <param name="exchangeName">Fanout 交换机名称</param>
            /// <param name="queueName">服务标识，用于生成队列名（如服务名）</param>
            /// <param name="handler">消息处理方法</param>
            /// <param name="autoAck">是否自动确认</param>
            /// <param name="isExclusive">是否排他队列（建议true，广播通常是临时消费）</param>
            /// <remarks>
            /// 此方法专为 Fanout 广播设计：
            /// 1. 每个消费者会创建唯一的临时队列
            /// 2. 自动绑定到指定 Fanout 交换机
            /// 3. routingKey 被忽略（传递空字符串）
            /// </remarks>
            public void StartConsumingBroadcast<T>(string exchangeName, string queueName, Func<T, Task> handler, bool autoAck = false, bool isExclusive = true) where T : class
            {
                _inner.StartConsumingBroadcast(GetTenantExchangeName(exchangeName), GetTenantQueueName(queueName ?? "default"), handler, autoAck);
            }

            #region 根据指定通道订阅消息
            public void StartConsuming<T>(IModel channel, string queueName, Func<T, Task> handler, bool autoAck = false) where T : class
            {
                _inner.StartConsuming(channel, GetTenantQueueName(queueName), handler, autoAck);
            }

            public void StartConsumingTopic<T>(IModel channel, string exchangeName, string routingKey, Func<T, Task> handler, bool autoAck = false) where T : class
            {
                _inner.StartConsumingTopic(channel, GetTenantExchangeName(exchangeName), GetTenantRoutingKeyName(routingKey), handler, autoAck);
            }

            public void StartConsumingTopic<T>(IModel channel, string exchangeName, string routingKey, string queueName, Func<T, Task> handler, bool autoAck = false) where T : class
            {
                _inner.StartConsumingTopic(channel, GetTenantExchangeName(exchangeName), GetTenantRoutingKeyName(routingKey), GetTenantQueueName(queueName), handler, autoAck);
            }

            public void StartConsumingExchange<T>(IModel channel, string exchangeName, string? queueName, string routingKey, Func<T, Task> handler, bool autoAck = false) where T : class
            {
                _inner.StartConsumingExchange(channel, GetTenantExchangeName(exchangeName), GetTenantQueueName(queueName ?? "default"), GetTenantRoutingKeyName(routingKey), handler, autoAck);
            }

            public void StartConsumingBroadcast<T>(IModel channel, string exchangeName, string queueName,
                Func<T, Task> handler, bool autoAck = false, bool isExclusive = true) where T : class
            {
                _inner.StartConsumingBroadcast(channel, GetTenantExchangeName(exchangeName), GetTenantQueueName(queueName ?? "default"), handler, autoAck);
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
                _inner.StopConsuming(GetTenantQueueName(queueName));
            }

        }
    }
}
