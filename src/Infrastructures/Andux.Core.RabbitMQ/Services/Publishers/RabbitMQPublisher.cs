using Andux.Core.RabbitMQ.Exceptions;
using Andux.Core.RabbitMQ.Interfaces;
using RabbitMQ.Client;
using System.Text.Json;

namespace Andux.Core.RabbitMQ.Services.Publishers
{
    /// <summary>
    /// RabbitMQ 发布者实现
    /// </summary>
    public class RabbitMQPublisher : IRabbitMQPublisher
    {
        private readonly IRabbitMQConnectionProvider _connectionProvider;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="connectionProvider"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public RabbitMQPublisher(IRabbitMQConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
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
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("队列名称不能为null或空", nameof(queueName));

            using var channel = _connectionProvider.CreateChannel();

            channel.QueueDeclare(
                queue: queueName,
                durable: persistent,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            PublishMessage(channel, string.Empty, queueName, message, persistent);
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
            if (string.IsNullOrWhiteSpace(exchangeName))
                throw new ArgumentException("Exchange名称不能为null或空", nameof(exchangeName));

            using var channel = _connectionProvider.CreateChannel();

            PublishMessage(channel, exchangeName, routingKey, message, persistent);
        }

        /// <summary>
        /// 批量发布消息到交换机，比单条发布有更高吞吐量。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exchangeName">目标交换机名称</param>
        /// <param name="messages">消息集合</param>
        /// <param name="persistent">是否持久化消息</param>
        [Obsolete]
        public void PublishBatch<T>(string exchangeName, IEnumerable<(string RoutingKey, T Message)> messages, bool persistent = true) where T : class
        {
            if (messages == null || !messages.Any())
                throw new ArgumentException("消息不能为null或空", nameof(messages));

            using var channel = _connectionProvider.CreateChannel();
            var batch = channel.CreateBasicPublishBatch();

            foreach (var (routingKey, message) in messages)
            {
                var body = JsonSerializer.SerializeToUtf8Bytes(message, _jsonOptions);
                var properties = channel.CreateBasicProperties();
                ConfigureProperties(properties, persistent);

                batch.Add(
                    exchange: exchangeName,
                    routingKey: routingKey,
                    mandatory: false,
                    properties: properties,
                    body: body);
            }

            try
            {
                batch.Publish();
            }
            catch (Exception ex)
            {
                throw new RabbitMQException("发布批处理消息时出错", ex);
            }
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="exchange"></param>
        /// <param name="routingKey"></param>
        /// <param name="message"></param>
        /// <param name="persistent"></param>
        /// <exception cref="RabbitMQException"></exception>
        private void PublishMessage<T>(IModel channel, string exchange, string routingKey, T message, bool persistent) where T : class
        {
            try
            {
                var body = JsonSerializer.SerializeToUtf8Bytes(message, _jsonOptions);

                var properties = channel.CreateBasicProperties();
                ConfigureProperties(properties, persistent);

                channel.BasicPublish(
                    exchange: exchange,
                    routingKey: routingKey,
                    mandatory: false,
                    basicProperties: properties,
                    body: body);
            }
            catch (Exception ex)
            {
                throw new RabbitMQException("发布消息时出错", ex);
            }
        }

        /// <summary>
        /// 配置属性
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="persistent"></param>
        private void ConfigureProperties(IBasicProperties properties, bool persistent)
        {
            properties.Persistent = persistent;
            properties.DeliveryMode = persistent ? (byte)2 : (byte)1;
        }

    }
}
