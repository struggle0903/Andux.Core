namespace Andux.Core.EventBus
{
    public interface IEvent { }

    public interface IEventHandler<TEvent> where TEvent : IEvent
    {
        Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
    }

    public interface IEventBus
    {
        Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : IEvent;

        Task SubscribeAsync<TEvent, THandler>()
            where TEvent : IEvent
            where THandler : IEventHandler<TEvent>;
    }
}
