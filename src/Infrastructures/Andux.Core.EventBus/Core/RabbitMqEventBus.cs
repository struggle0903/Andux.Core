using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andux.Core.EventBus.Core
{
    public class RabbitMqEventBus : IEventBus
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly EventBusOptions _options;

        public RabbitMqEventBus(IServiceProvider serviceProvider,
            IOptions<EventBusOptions> options)
        {
            _serviceProvider = serviceProvider;
            _options = options.Value;
        }

        public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
        {
            var queue = _options.QueueName ?? typeof(TEvent).Name;
            // 使用 queue 发布消息

            throw new NotImplementedException();
        }

        public Task SubscribeAsync<TEvent, THandler>()
            where TEvent : IEvent
            where THandler : IEventHandler<TEvent>
        {
            throw new NotImplementedException();
        }
    }
}
