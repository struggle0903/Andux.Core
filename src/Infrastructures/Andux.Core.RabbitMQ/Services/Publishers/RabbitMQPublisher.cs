using Andux.Core.RabbitMQ.Exceptions;
using Andux.Core.RabbitMQ.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Encodings.Web;
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
                WriteIndented = false,

                // 加上后不会出现 \u0022 编码问题
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
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
        /// 发布主题消息
        /// 支持 Topic Exchange
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exchangeName">目标交换机名称</param>
        /// <param name="routingKey">路由键</param>
        /// <param name="message">消息</param>
        /// <param name="persistent">是否持久化消息</param>
        public void PublishTopic<T>(string exchangeName, string routingKey, T message, bool persistent = true) where T : class
        {
            using var channel = _connectionProvider.CreateChannel();
            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);
            PublishMessage(channel, exchangeName, routingKey, message, persistent);
        }

        /// <summary>
        /// 发布广播消息（Fanout 交换机，所有绑定队列都会收到）, 持久化消息
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="exchangeName">目标交换机名称</param>
        /// <param name="message">要广播的消息</param>
        /// <param name="persistent">是否持久化消息</param>
        public void PublishBroadcast<T>(string exchangeName, T message, bool persistent = true) where T : class
        {
            using var channel = _connectionProvider.CreateChannel();

            // Fanout 交换机：忽略路由键，广播到所有绑定队列
            // channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout, durable: true);

            try
            {
                // 1. 先尝试被动声明（只检查，不创建）
                channel.ExchangeDeclarePassive(exchangeName);

                // 如果执行到这里，说明交换机已存在
                channel.ExchangeDeclare(
                    exchange: exchangeName,
                    type: ExchangeType.Fanout,
                    durable: true,     // 与现有交换机参数保持一致很重要！
                    autoDelete: false);
            }
            catch (OperationInterruptedException ex) when (ex.Message.Contains("NOT_FOUND"))
            {
                // 2. 交换机不存在，创建它（Fanout 类型）
                channel.ExchangeDeclare(
                    exchange: exchangeName,
                    type: ExchangeType.Fanout,
                    durable: true,     // 与现有交换机参数保持一致很重要！
                    autoDelete: false);
            }
            catch (OperationInterruptedException ex) when (ex.Message.Contains("PRECONDITION_FAILED"))
            {
                // 3. 交换机已存在但参数不匹配
                throw new InvalidOperationException(
                    $"交换机 '{exchangeName}' 已存在，但参数与请求的不匹配。\n" +
                    $"现有类型可能是 'topic'，而你在尝试创建 'fanout'。\n" +
                    $"请删除或重命名交换机后再试。", ex);
            }

            // Fanout 交换机忽略 routingKey，传递空字符串
            PublishMessage(channel, exchangeName, string.Empty, message, persistent);
        }

        /// <summary>
        /// 发布广播消息到临时广播交换机，不持久化
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
            using var channel = _connectionProvider.CreateChannel();

            // autoDelete: 临时广播通常设为true，连接断开后自动清理
            channel.ExchangeDeclare(
                exchange: exchangeName,
                type: ExchangeType.Fanout,
                durable: false,    // 临时广播不需要持久化
                autoDelete: autoDelete,
                arguments: null);

            PublishMessage(channel, exchangeName, string.Empty, message, persistent: false);
        }

        #region 根据Connection连接对象发布消息

        /// <summary>
        /// 发布消息到指定队列（使用连接创建临时通道）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">连接对象</param>
        /// <param name="queueName">目标交换机名称</param>
        /// <param name="message">消息</param>
        /// <param name="persistent">是否持久化消息</param>
        public void PublishToQueue<T>(IConnection connection, string queueName, T message, bool persistent = true) where T : class
        {
            using var channel = connection.CreateModel();
            PublishMessage(channel, string.Empty, queueName, message, persistent);
        }

        /// <summary>
        /// 发布消息到指定交换机（使用连接创建临时通道）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">连接对象</param>
        /// <param name="exchangeName">目标交换机名称</param>
        /// <param name="routingKey">路由键</param>
        /// <param name="message">消息</param>
        /// <param name="persistent">是否持久化消息</param>
        public void PublishToExchange<T>(IConnection connection, string exchangeName, string routingKey, T message, bool persistent = true) where T : class
        {
            using var channel = connection.CreateModel();
            PublishMessage(channel, exchangeName, routingKey, message, persistent);
        }

        /// <summary>
        /// 批量发布消息到交换机（使用连接创建临时通道）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">连接对象</param>
        /// <param name="exchangeName">目标交换机名称</param>
        /// <param name="messages">消息集合</param>
        /// <param name="persistent">是否持久化消息</param>
        public void PublishBatch<T>(IConnection connection, string exchangeName, IEnumerable<(string RoutingKey, T Message)> messages, bool persistent = true) where T : class
        {
            using var channel = connection.CreateModel();
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
        /// 发布主题消息（使用连接创建临时通道）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">连接对象</param>
        /// <param name="exchangeName">目标交换机名称</param>
        /// <param name="routingKey">路由键</param>
        /// <param name="message">消息</param>
        /// <param name="persistent">是否持久化消息</param>
        public void PublishTopic<T>(IConnection connection, string exchangeName, string routingKey, T message, bool persistent = true) where T : class
        {
            using var channel = connection.CreateModel();
            PublishMessage(channel, exchangeName, routingKey, message, persistent);
        }

        #endregion

        #region 私有方法

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
                byte[] body;

                // 根据类型决定序列化方式
                if (typeof(T) == typeof(string))
                {
                    // 对于 string 类型，检查是否为有效的 JSON
                    string stringMessage = message as string;
                    if (IsValidJson(stringMessage))
                    {
                        // 如果是 JSON 字符串，直接编码（不额外序列化）
                        body = Encoding.UTF8.GetBytes(stringMessage);
                    }
                    else
                    {
                        // 如果是普通字符串，作为 JSON 字符串序列化
                        body = JsonSerializer.SerializeToUtf8Bytes(message, _jsonOptions);
                    }
                }
                else if (typeof(T).IsPrimitive || message is ValueType)
                {
                    // 对于基本类型，直接序列化为 JSON
                    body = JsonSerializer.SerializeToUtf8Bytes(message, _jsonOptions);
                }
                else
                {
                    // 对于复杂对象，序列化为 JSON
                    body = JsonSerializer.SerializeToUtf8Bytes(message, _jsonOptions);
                }

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

        /// <summary>
        /// 检查字符串是否为有效的json
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private bool IsValidJson(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return false;

            try
            {
                var trimmed = str.Trim();
                return (trimmed.StartsWith("{") && trimmed.EndsWith("}")) ||
                       (trimmed.StartsWith("[") && trimmed.EndsWith("]"));
            }
            catch
            {
                return false;
            }
        }

        #endregion

    }
}
