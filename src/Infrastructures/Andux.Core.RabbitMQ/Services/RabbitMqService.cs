using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Andux.Core.RabbitMQ.Options;

namespace Andux.Core.RabbitMQ.Services
{
    /// <summary>
    /// RabbitMQ 管理器，支持多用户多连接。
    /// </summary>
    public class RabbitMqService: IRabbitMqService, IDisposable
    {
        // 存储多个用户的连接和通道
        private readonly Dictionary<string, IConnection> _connections = new();
        private readonly Dictionary<string, IModel> _channels = new();
        private readonly ILogger<RabbitMqService> _logger;

        public RabbitMqService(IOptions<RabbitMqOptions> options, ILogger<RabbitMqService> logger)
        {
            _logger = logger;

            // 配置多个用户连接
            foreach (var user in options.Value.Users)
            {
                // 设置连接工厂
                var factory = new ConnectionFactory
                {
                    HostName = user.HostName,
                    Port = user.Port,
                    UserName = user.UserName,
                    Password = user.Password,
                    VirtualHost = user.VirtualHost,
                    ClientProvidedName = user.ClientProvidedName
                };

                // 创建连接
                var connection = factory.CreateConnection($"{user.UserName}-Connection");
                var channel = connection.CreateModel();  // 获取 IModel（通道）

                _connections[user.UserName] = connection;
                _channels[user.UserName] = channel;

                _logger.LogInformation($"RabbitMQ connection for user {user.UserName} established.");
            }
        }

        /// <summary>
        /// 发布消息到指定用户的交换机
        /// </summary>
        public async Task PublishAsync(string userName, string exchange, string routingKey, string message)
        {
            if (!_channels.ContainsKey(userName))
                throw new ArgumentException($"RabbitMQ user {userName} is not configured.");

            var channel = _channels[userName];
            var body = Encoding.UTF8.GetBytes(message);

            // 发布消息到交换机
            channel.BasicPublish(exchange, routingKey, null, body);
            await Task.CompletedTask;
        }

        /// <summary>
        /// 订阅指定用户的消息队列
        /// </summary>
        public void Subscribe(string userName, string queue, string exchange, string routingKey, Action<string> onMessageReceived)
        {
            if (!_channels.ContainsKey(userName))
                throw new ArgumentException($"RabbitMQ user {userName} is not configured.");

            var channel = _channels[userName];

            // 声明交换机和队列
            channel.ExchangeDeclare(exchange, "direct", durable: true, autoDelete: false);
            channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(queue, exchange, routingKey);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                try
                {
                    onMessageReceived(message);
                    channel.BasicAck(ea.DeliveryTag, false); // 确认消息
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing message: {ex.Message}");
                    channel.BasicNack(ea.DeliveryTag, false, true); // 处理失败则重新排队
                }
            };

            // 消费消息
            channel.BasicConsume(queue, autoAck: false, consumer);
            _logger.LogInformation($"Subscribed to queue {queue} for user {userName}");
        }

        public void Dispose()
        {
            // 释放所有的连接和通道
            foreach (var connection in _connections.Values)
            {
                connection.Dispose();
            }

            foreach (var channel in _channels.Values)
            {
                channel.Dispose();
            }
        }

    }
}
