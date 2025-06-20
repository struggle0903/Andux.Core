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
        /// <param name="queueName"></param>
        /// <param name="handler"></param>
        /// <param name="autoAck"></param>
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
        /// 开始消费指定租户的队列
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="tenantId">租户ID</param>
        /// <param name="queueName">队列名称</param>
        /// <param name="handler">消息处理委托</param>
        /// <param name="autoAck">是否自动确认</param>
        public void StartConsuming<T>(string tenantId, string queueName, Func<T, Task> handler, bool autoAck = false) where T : class
        {
            // 参数验证
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("队列名称不能为null或空", nameof(queueName));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            // 生成租户感知的队列Key
            var tenantQueueKey = GetTenantQueueKey(tenantId, queueName);

            if (_consumers.ContainsKey(tenantQueueKey))
                throw new InvalidOperationException($"租户[{tenantId}]已占用队列: {queueName}");

            // 获取租户专属通道
            var channel = _connectionProvider.CreateChannel(tenantId);
            if (!_channels.TryAdd(tenantQueueKey, channel))
                throw new RabbitMQException($"为租户[{tenantId}]队列添加通道失败: {queueName}");

            // 声明队列（租户隔离的队列名称）
            var tenantQueueName = GetTenantSpecificQueueName(tenantId, queueName);
            channel.QueueDeclare(
                queue: tenantQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            channel.BasicQos(0, 1, false);

            // 创建消费者
            var consumer = new AsyncEventingBasicConsumer(channel);
            if (!_consumers.TryAdd(tenantQueueKey, consumer))
                throw new RabbitMQException($"未能为租户[{tenantId}]队列添加消费者: {queueName}");

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
                    throw new RabbitMQException($"处理租户[{tenantId}]队列消息时出错: {queueName}", ex);
                }
            };

            channel.BasicConsume(
                queue: tenantQueueName,
                autoAck: autoAck,
                consumer: consumer);
        }

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

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            StopConsuming();
            GC.SuppressFinalize(this);
        }

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

    }
}
