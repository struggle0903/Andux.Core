//using Andux.Core.EventBus.Abstractions;
//using Microsoft.EntityFrameworkCore.Metadata;
//using RabbitMQ.Client;
//using RabbitMQ.Client.Events;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;

//namespace Andux.Core.EventBus.RabbitMQ
//{
//    public class RabbitMqEventBus : IEventBus, IDisposable
//    {
//        private readonly IModel _channel;
//        private readonly RabbitMqOptions _options;
//        private readonly IServiceProvider _provider;

//        public RabbitMqEventBus(IOptions<RabbitMqOptions> options, IServiceProvider provider)
//        {
//            _options = options.Value;
//            _provider = provider;

//            var factory = new ConnectionFactory { HostName = _options.HostName };
//            var connection = factory.CreateConnection();
//            _channel = connection.CreateModel();

//            _channel.ExchangeDeclare(exchange: _options.ExchangeName, type: ExchangeType.Fanout);
//        }

//        public void Publish<TEvent>(TEvent @event) where TEvent : class
//        {
//            var body = JsonSerializer.SerializeToUtf8Bytes(@event);
//            _channel.BasicPublish(exchange: _options.ExchangeName, routingKey: "", body: body);
//        }

//        public void Subscribe<TEvent>() where TEvent : class
//        {
//            var queueName = _channel.QueueDeclare().QueueName;
//            _channel.QueueBind(queue: queueName, exchange: _options.ExchangeName, routingKey: "");

//            var consumer = new AsyncEventingBasicConsumer(_channel);
//            consumer.Received += async (_, ea) =>
//            {
//                var body = ea.Body.ToArray();
//                var message = JsonSerializer.Deserialize<TEvent>(body);
//                if (message != null)
//                {
//                    using var scope = _provider.CreateScope();
//                    var dispatcher = scope.ServiceProvider.GetRequiredService<EventBusDispatcher>();
//                    await dispatcher.DispatchAsync(message);
//                }
//            };

//            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
//        }

//        public void Dispose()
//        {
//            _channel?.Dispose();
//        }
//    }
//}
