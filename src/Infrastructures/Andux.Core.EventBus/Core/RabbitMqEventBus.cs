using Andux.Core.EventBus.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace Andux.Core.EventBus.Core
{
    /// <summary>
    /// 基于 RabbitMQ 的事件总线实现，支持发布和订阅异步事件。
    /// </summary>
    public class RabbitMqEventBus : IRabbitMQEventBus, IDisposable
    {
        /// <summary>
        /// 用于创建事件处理器的服务提供者，支持依赖注入范围。
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 事件总线配置选项，包含 RabbitMQ 连接相关信息。
        /// </summary>
        private readonly AnduxEventBusOptions _options;

        /// <summary>
        /// 延迟初始化 RabbitMQ 连接对象，避免构造函数连接，提升启动性能。
        /// </summary>
        private readonly Lazy<IConnection> _connectionLazy;

        /// <summary>
        /// 延迟初始化 RabbitMQ 通道对象，用于消息操作。
        /// </summary>
        private readonly Lazy<IModel> _channelLazy;

        /// <summary>
        /// 标识资源是否已经释放，防止重复释放。
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 构造函数，注入服务提供者和配置选项，初始化延迟连接和通道。
        /// </summary>
        public RabbitMqEventBus(IServiceProvider serviceProvider, IOptions<AnduxEventBusOptions> options)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            ValidateOptions(_options);

            _connectionLazy = new Lazy<IConnection>(CreateConnection, true);
            _channelLazy = new Lazy<IModel>(() => _connectionLazy.Value.CreateModel(), true);
        }

        /// <summary>
        /// 校验配置项是否满足建立 RabbitMQ 连接的最低条件。
        /// </summary>
        private void ValidateOptions(AnduxEventBusOptions opt)
        {
            if (string.IsNullOrWhiteSpace(opt.HostName))
                throw new ArgumentException("RabbitMQ HostName is required.");
            if (string.IsNullOrWhiteSpace(opt.UserName))
                throw new ArgumentException("RabbitMQ UserName is required.");
            if (string.IsNullOrWhiteSpace(opt.Password))
                throw new ArgumentException("RabbitMQ Password is required.");
        }

        /// <summary>
        /// 创建并返回 RabbitMQ 连接实例。
        /// </summary>
        private IConnection CreateConnection()
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port ?? 5672,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost ?? "/",
                DispatchConsumersAsync = true // 支持异步消费者事件处理
            };

            return factory.CreateConnection();
        }

        /// <summary>
        /// 获取 RabbitMQ 通道实例，惰性加载。
        /// </summary>
        private IModel Channel => _channelLazy.Value;

        /// <summary>
        /// 发布事件消息到指定队列。
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="event">事件实例</param>
        /// <param name="cancellationToken">取消令牌（暂未使用）</param>
        public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
        {
            // 队列名，默认用事件类型名称
            var queue = typeof(TEvent).Name + "-queue";
            return BasicPublishAsync(@event, queue, cancellationToken);
        }

        /// <summary>
        /// 订阅指定事件类型，并通过指定处理器处理消息。
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <typeparam name="THandler">事件处理器类型</typeparam>
        public Task SubscribeAsync<TEvent, THandler>()
            where TEvent : IEvent
            where THandler : IEventHandler<TEvent>
        {
            var queue = typeof(TEvent).Name + "-queue";
            return BasicConsume<TEvent, THandler>(queue);
        }

        /// <summary>
        /// 发布事件到指定队列，允许显式指定队列名。
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="event"></param>
        /// <param name="queueName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task PublishAsync<TEvent>(TEvent @event, string queueName, CancellationToken cancellationToken = default) where TEvent : IEvent
        {
            return BasicPublishAsync(@event, queueName, cancellationToken);
        }

        /// <summary>
        /// 订阅指定事件类型的处理器，允许显式指定队列名。
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="THandler"></typeparam>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public Task SubscribeAsync<TEvent, THandler>(string queueName)
            where TEvent : IEvent
            where THandler : IEventHandler<TEvent>
        {
            return BasicConsume<TEvent, THandler>(queueName);
        }

        /// <summary>
        /// 释放托管和非托管资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 资源释放具体实现，防止重复释放。
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // 释放 RabbitMQ 通道和连接
                if (_channelLazy.IsValueCreated)
                    _channelLazy.Value.Dispose();

                if (_connectionLazy.IsValueCreated)
                    _connectionLazy.Value.Dispose();
            }

            _disposed = true;
        }

        #region 私有方法

        /// <summary>
        /// 发送消息到队列
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="event">事件对象</param>
        /// <param name="queueName">队列名</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private Task BasicPublishAsync<TEvent>(TEvent @event, string queueName, CancellationToken cancellationToken = default) where TEvent : IEvent
        {
            // 声明队列（幂等）
            Channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

            // 序列化事件为 JSON 字节流
            var body = JsonSerializer.SerializeToUtf8Bytes(@event);

            // 消息属性，设置消息持久化
            var props = Channel.CreateBasicProperties();
            props.Persistent = true;

            // 发送消息到队列
            Channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: props, body: body);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 订阅指定事件类型的处理器，开始消费队列中的消息。
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="THandler"></typeparam>
        /// <param name="queueName"></param>
        /// <returns></returns>
        private Task BasicConsume<TEvent, THandler>(string queueName) where TEvent : IEvent where THandler : IEventHandler<TEvent>
        {
            // 声明队列
            Channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

            // 创建异步事件消费者
            var consumer = new AsyncEventingBasicConsumer(Channel);
            consumer.Received += async (model, ea) =>
            {
                // 创建新的作用域，支持依赖注入的范围生命周期
                using var scope = _serviceProvider.CreateScope();

                // 获取事件处理器实例
                var handler = scope.ServiceProvider.GetService<THandler>();
                if (handler == null)
                {
                    Console.WriteLine($"未找到事件处理器：{typeof(THandler).FullName}");
                    return;
                }

                // 反序列化消息为事件对象
                var message = ea.Body.ToArray();

                try
                {
                    var @event = JsonSerializer.Deserialize<TEvent>(message);
                    if (@event != null)
                    {
                        await handler.HandleAsync(@event);
                        Channel.BasicAck(ea.DeliveryTag, false); // 放在里面更安全
                    }
                    else
                    {
                        Console.WriteLine("消息反序列化失败，未能处理。");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"处理事件时发生异常：{ex.Message}");
                    // 一旦失败就丢了。建议在 catch 中用 BasicNack 做补偿（可选）
                    Channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            // 开始消费队列中的消息，不自动确认，交由业务处理后手动确认
            Channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }

        #endregion

    }
}
