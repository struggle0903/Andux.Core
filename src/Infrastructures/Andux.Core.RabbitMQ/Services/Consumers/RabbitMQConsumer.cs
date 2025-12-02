using Andux.Core.RabbitMQ.Exceptions;
using Andux.Core.RabbitMQ.Interfaces;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Andux.Core.RabbitMQ.Services.Consumers
{
    /// <summary>
    /// RabbitMQ 消费者实现
    /// </summary>
    public class RabbitMQConsumer : IRabbitMQConsumer, IDisposable
    {
        private readonly IRabbitMQConnectionProvider _connectionProvider;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ConcurrentDictionary<string, IModel> _channels;
        private readonly ConcurrentDictionary<string, AsyncEventingBasicConsumer> _consumers;
        private bool _disposed;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionProvider"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public RabbitMQConsumer(IRabbitMQConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _channels = new ConcurrentDictionary<string, IModel>();
            _consumers = new ConcurrentDictionary<string, AsyncEventingBasicConsumer>();
        }

        /// <summary>
        /// 开始消费
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queueName">队列名</param>
        /// <param name="handler">处理函数</param>
        /// <param name="autoAck">是否自动确认</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="RabbitMQException"></exception>
        public void StartConsuming<T>(string queueName, Func<T, Task> handler, bool autoAck = false) where T : class
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("队列名称不能为null或空", nameof(queueName));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (_consumers.ContainsKey(queueName))
                throw new InvalidOperationException($"已占用队列: {queueName}");

            var channel = _connectionProvider.CreateChannel();
            if (!_channels.TryAdd(queueName, channel))
                throw new RabbitMQException($"为队列添加通道失败: {queueName}");

            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            channel.BasicQos(0, 1, false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            if (!_consumers.TryAdd(queueName, consumer))
                throw new RabbitMQException($"未能为队列添加消费者: {queueName}");

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var message = JsonSerializer.Deserialize<T>(ea.Body.Span, _jsonOptions);
                    if (message != null)
                    {
                        await handler(message);
                    }

                    if (!autoAck)
                    {
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
                catch (Exception ex)
                {
                    if (!autoAck)
                    {
                        channel.BasicReject(ea.DeliveryTag, false);
                    }
                    throw new RabbitMQException($"处理来自队列的消息时出错: {queueName}", ex);
                }
            };

            channel.BasicConsume(
                queue: queueName,
                autoAck: autoAck,
                consumer: consumer);
        }

        /// <summary>
        /// 订阅 Topic Exchange 消息（随机队列名）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exchangeName">Topic 交换机名称</param>
        /// <param name="routingKey">路由键，可使用 * 或 # 通配符</param>
        /// <param name="handler">消息处理方法</param>
        /// <param name="autoAck">是否自动确认</param>
        public void StartConsumingTopic<T>(string exchangeName, string routingKey, Func<T, Task> handler, bool autoAck = false) where T : class
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
                throw new ArgumentException("Exchange 名称不能为 null 或空", nameof(exchangeName));

            if (string.IsNullOrWhiteSpace(routingKey))
                throw new ArgumentException("RoutingKey 不能为 null 或空", nameof(routingKey));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var channel = _connectionProvider.CreateChannel();

            // 声明 Topic Exchange
            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);

            // 随机队列名
            var queueName = channel.QueueDeclare().QueueName;

            // 绑定队列到 Topic Exchange，并使用路由键
            channel.QueueBind(queueName, exchangeName, routingKey);

            channel.BasicQos(0, 1, false);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var message = JsonSerializer.Deserialize<T>(ea.Body.Span, _jsonOptions);
                    if (message != null)
                    {
                        await handler(message);
                    }

                    if (!autoAck)
                    {
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
                catch (Exception)
                {
                    if (!autoAck)
                    {
                        channel.BasicReject(ea.DeliveryTag, false);
                    }
                    // 可以考虑记录日志
                }
            };

            channel.BasicConsume(queue: queueName, autoAck: autoAck, consumer: consumer);
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
        public void StartConsumingTopic<T>(string exchangeName, string routingKey, string queueName, Func<T, Task> handler, bool autoAck = false) where T : class
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
                throw new ArgumentException("Exchange 名称不能为 null 或空", nameof(exchangeName));

            if (string.IsNullOrWhiteSpace(routingKey))
                throw new ArgumentException("RoutingKey 不能为 null 或空", nameof(routingKey));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var channel = _connectionProvider.CreateChannel();

            // 声明 Topic Exchange
            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);

            channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(queueName, exchangeName, routingKey);

            channel.BasicQos(0, 1, false);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var message = JsonSerializer.Deserialize<T>(ea.Body.Span, _jsonOptions);
                    if (message != null)
                    {
                        await handler(message);
                    }

                    if (!autoAck)
                    {
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
                catch (Exception)
                {
                    if (!autoAck)
                    {
                        channel.BasicReject(ea.DeliveryTag, false);
                    }
                    // 可以考虑记录日志
                }
            };

            channel.BasicConsume(queue: queueName, autoAck: autoAck, consumer: consumer);
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
        public void StartConsumingExchange<T>(string exchangeName, string? queueName, string routingKey,
            Func<T, Task> handler, bool autoAck = false) where T : class
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
                throw new ArgumentException("Exchange 名称不能为 null 或空", nameof(exchangeName));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var channel = _connectionProvider.CreateChannel();

            // 声明 Exchange
            // 如果你使用 Topic 类型，可根据需要传参数
            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);

            // 如果 queueName 为 null，则创建临时随机队列（广播模式）
            if (string.IsNullOrWhiteSpace(queueName))
            {
                queueName = channel.QueueDeclare().QueueName;
            }
            else
            {
                // 固定队列，持久化
                channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
            }

            // 绑定队列到 Exchange
            channel.QueueBind(queueName, exchangeName, routingKey);

            // 限流：每个消费者最多处理 1 条未确认消息
            channel.BasicQos(0, 1, false);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var message = JsonSerializer.Deserialize<T>(ea.Body.Span, _jsonOptions);
                    if (message != null)
                    {
                        await handler(message);
                    }

                    if (!autoAck)
                    {
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
                catch (Exception)
                {
                    if (!autoAck)
                    {
                        channel.BasicReject(ea.DeliveryTag, false);
                    }
                    // 可记录日志
                }
            };

            channel.BasicConsume(queue: queueName, autoAck: autoAck, consumer: consumer);
        }

        #region 根据IModel订阅消息 

        /// <summary>
        /// 开始消费
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queueName">队列名</param>
        /// <param name="handler">处理函数</param>
        /// <param name="channel">消息通道</param>
        /// <param name="autoAck">是否自动确认</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="RabbitMQException"></exception>
        public void StartConsuming<T>(IModel channel, string queueName, Func<T, Task> handler, bool autoAck = false) where T : class
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("队列名称不能为null或空", nameof(queueName));

            if (channel == null)
                throw new ArgumentNullException("根据IModel订阅消息[StartConsuming]方法参数错误: channel 为 null");

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (_consumers.ContainsKey(queueName))
                throw new InvalidOperationException($"已占用队列: {queueName}");

            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            channel.BasicQos(0, 1, false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            if (!_consumers.TryAdd(queueName, consumer))
                throw new RabbitMQException($"未能为队列添加消费者: {queueName}");

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var message = JsonSerializer.Deserialize<T>(ea.Body.Span, _jsonOptions);
                    if (message != null)
                    {
                        await handler(message);
                    }

                    if (!autoAck)
                    {
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
                catch (Exception ex)
                {
                    if (!autoAck)
                    {
                        channel.BasicReject(ea.DeliveryTag, false);
                    }
                    throw new RabbitMQException($"处理来自队列的消息时出错: {queueName}", ex);
                }
            };

            channel.BasicConsume(
                queue: queueName,
                autoAck: autoAck,
                consumer: consumer);
        }

        /// <summary>
        /// 订阅 Topic Exchange 消息（随机队列名）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exchangeName">Topic 交换机名称</param>
        /// <param name="routingKey">路由键，可使用 * 或 # 通配符</param>
        /// <param name="handler">消息处理方法</param>
        /// <param name="channel">消息通道</param>
        /// <param name="autoAck">是否自动确认</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="RabbitMQException"></exception>
        public void StartConsumingTopic<T>(IModel? channel, string exchangeName, string routingKey, Func<T, Task> handler, bool autoAck = false) where T : class
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
                throw new ArgumentException("Exchange 名称不能为 null 或空", nameof(exchangeName));

            if (channel == null)
                throw new ArgumentNullException("根据IModel订阅消息[StartConsumingTopic]方法参数错误: channel 为 null");

            if (string.IsNullOrWhiteSpace(routingKey))
                throw new ArgumentException("RoutingKey 不能为 null 或空", nameof(routingKey));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            // 声明 Topic Exchange
            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);

            // 随机队列名
            var queueName = channel.QueueDeclare().QueueName;

            // 绑定队列到 Topic Exchange，并使用路由键
            channel.QueueBind(queueName, exchangeName, routingKey);

            channel.BasicQos(0, 1, false);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var message = JsonSerializer.Deserialize<T>(ea.Body.Span, _jsonOptions);
                    if (message != null)
                    {
                        await handler(message);
                    }

                    if (!autoAck)
                    {
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
                catch (Exception)
                {
                    if (!autoAck)
                    {
                        channel.BasicReject(ea.DeliveryTag, false);
                    }
                    // 可以考虑记录日志
                }
            };

            channel.BasicConsume(queue: queueName, autoAck: autoAck, consumer: consumer);
        }

        /// <summary>
        /// 订阅 Topic Exchange 消息（自定义队列名）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exchangeName">Topic 交换机名称</param>
        /// <param name="routingKey">路由键，可使用 * 或 # 通配符</param>
        /// <param name="queueName">用户订阅队列名</param>
        /// <param name="handler">消息处理方法</param>
        /// <param name="channel">消息通道</param>
        /// <param name="autoAck">是否自动确认</param>
        public void StartConsumingTopic<T>(IModel? channel, string exchangeName, string routingKey, string queueName, Func<T, Task> handler, bool autoAck = false) where T : class
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
                throw new ArgumentException("Exchange 名称不能为 null 或空", nameof(exchangeName));

            if (channel == null)
                throw new ArgumentNullException("根据IModel订阅消息[StartConsumingTopic]方法参数错误: channel 为 null");

            if (string.IsNullOrWhiteSpace(routingKey))
                throw new ArgumentException("RoutingKey 不能为 null 或空", nameof(routingKey));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            // 声明 Topic Exchange
            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);

            channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(queueName, exchangeName, routingKey);

            channel.BasicQos(0, 1, false);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var message = JsonSerializer.Deserialize<T>(ea.Body.Span, _jsonOptions);
                    if (message != null)
                    {
                        await handler(message);
                    }

                    if (!autoAck)
                    {
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
                catch (Exception)
                {
                    if (!autoAck)
                    {
                        channel.BasicReject(ea.DeliveryTag, false);
                    }
                    // 可以考虑记录日志
                }
            };

            channel.BasicConsume(queue: queueName, autoAck: autoAck, consumer: consumer);
        }

        /// <summary>
        /// 开始消费 Exchange 消息（支持 Topic 或 Fanout）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="exchangeName">Exchange 名称</param>
        /// <param name="queueName">可选：固定队列名。如果为 null 或空，则创建临时随机队列，用于广播模式</param>
        /// <param name="routingKey">路由键，可使用 * 或 # 通配符，Fanout 可为空</param>
        /// <param name="handler">消息处理方法</param>
        /// <param name="channel">消息通道</param>
        /// <param name="autoAck">是否自动确认</param>
        public void StartConsumingExchange<T>(IModel? channel, string exchangeName, string? queueName, string routingKey,
            Func<T, Task> handler, bool autoAck = false) where T : class
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
                throw new ArgumentException("Exchange 名称不能为 null 或空", nameof(exchangeName));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            // 声明 Exchange
            // 如果你使用 Topic 类型，可根据需要传参数
            channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);

            // 如果 queueName 为 null，则创建临时随机队列（广播模式）
            if (string.IsNullOrWhiteSpace(queueName))
            {
                queueName = channel.QueueDeclare().QueueName;
            }
            else
            {
                // 固定队列，持久化
                channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
            }

            // 绑定队列到 Exchange
            channel.QueueBind(queueName, exchangeName, routingKey);

            // 限流：每个消费者最多处理 1 条未确认消息
            channel.BasicQos(0, 1, false);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var message = JsonSerializer.Deserialize<T>(ea.Body.Span, _jsonOptions);
                    if (message != null)
                    {
                        await handler(message);
                    }

                    if (!autoAck)
                    {
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
                catch (Exception)
                {
                    if (!autoAck)
                    {
                        channel.BasicReject(ea.DeliveryTag, false);
                    }
                    // 可记录日志
                }
            };

            channel.BasicConsume(queue: queueName, autoAck: autoAck, consumer: consumer);
        }

        #endregion

        /// <summary>
        /// 停止消费
        /// </summary>
        public void StopConsuming()
        {
            foreach (var queueName in _consumers.Keys.ToList())
            {
                StopConsuming(queueName);
            }
        }

        /// <summary>
        /// 停止指定消费者
        /// </summary>
        /// <param name="queueName"></param>
        /// <exception cref="RabbitMQException"></exception>
        public void StopConsuming(string queueName)
        {
            if (_consumers.TryRemove(queueName, out var consumer) &&
                _channels.TryRemove(queueName, out var channel))
            {
                try
                {
                    if (channel.IsOpen)
                    {
                        channel.Close();
                    }
                }
                catch (Exception ex)
                {
                    throw new RabbitMQException($"停止队列的消费者时出错: {queueName}", ex);
                }
                finally
                {
                    channel.Dispose();
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            StopConsuming();
            GC.SuppressFinalize(this);
        }

        #region 私有方法

        /// <summary>
        /// 辅助方法：生成租户队列唯一Key
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        private string GetTenantQueueKey(string tenantId, string queueName)
        {
            return $"tenant_{tenantId ?? "default"}_queue_{queueName}";
        }

        /// <summary>
        /// 辅助方法：生成租户专属队列名称
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="baseQueueName"></param>
        /// <returns></returns>
        private string GetTenantSpecificQueueName(string tenantId, string baseQueueName)
        {
            return tenantId != null ? $"{tenantId}_{baseQueueName}" : baseQueueName;
        }

        #endregion

    }
}
