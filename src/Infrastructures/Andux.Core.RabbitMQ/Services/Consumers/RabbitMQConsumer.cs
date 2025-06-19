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

        public RabbitMQConsumer(IRabbitMQConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _channels = new ConcurrentDictionary<string, IModel>();
            _consumers = new ConcurrentDictionary<string, AsyncEventingBasicConsumer>();
        }

        public void StartConsuming<T>(string queueName, Func<T, Task> handler, bool autoAck = false) where T : class
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue name cannot be null or empty", nameof(queueName));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (_consumers.ContainsKey(queueName))
                throw new InvalidOperationException($"Already consuming queue: {queueName}");

            var channel = _connectionProvider.CreateChannel();
            if (!_channels.TryAdd(queueName, channel))
                throw new RabbitMQException($"Failed to add channel for queue: {queueName}");

            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            channel.BasicQos(0, 1, false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            if (!_consumers.TryAdd(queueName, consumer))
                throw new RabbitMQException($"Failed to add consumer for queue: {queueName}");

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
                    throw new RabbitMQException($"Error processing message from queue: {queueName}", ex);
                }
            };

            channel.BasicConsume(
                queue: queueName,
                autoAck: autoAck,
                consumer: consumer);
        }



        public void StopConsuming()
        {
            foreach (var queueName in _consumers.Keys.ToList())
            {
                StopConsuming(queueName);
            }
        }

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
                    throw new RabbitMQException($"Error stopping consumer for queue: {queueName}", ex);
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

    }
}
