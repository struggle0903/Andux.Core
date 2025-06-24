using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Andux.Core.EventBus.Core
{
    public class InMemoryEventBus : IEventBus
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<Type, List<Type>> _handlers = new();

        public InMemoryEventBus(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task SubscribeAsync<TEvent, THandler>()where TEvent : IEvent where THandler : IEventHandler<TEvent>
        {
            var eventType = typeof(TEvent);
            var handlerType = typeof(THandler);

            _handlers.AddOrUpdate(eventType,
                _ => new() { handlerType },
                (_, existing) =>
                {
                    if (!existing.Contains(handlerType))
                        existing.Add(handlerType);
                    return existing;
                });

            return Task.CompletedTask;
        }

        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : IEvent
        {
            if (_handlers.TryGetValue(typeof(TEvent), out var handlerTypes))
            {
                using var scope = _serviceProvider.CreateScope();
                foreach (var handlerType in handlerTypes)
                {
                    var handler = (IEventHandler<TEvent>)scope.ServiceProvider.GetRequiredService(handlerType);
                    await handler.HandleAsync(@event, cancellationToken);
                }
            }
        }

    }
}
