namespace Andux.Core.EventBus.Middleware
{
    public interface IEventMiddleware
    {
        Task InvokeAsync<TEvent>(TEvent @event, Func<Task> next) where TEvent : IEvent;
    }
}
