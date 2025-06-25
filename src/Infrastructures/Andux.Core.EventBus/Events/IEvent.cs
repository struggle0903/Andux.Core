namespace Andux.Core.EventBus.Events
{
    /// <summary>
    /// 事件接口标记
    /// </summary>
    public interface IEvent { }

    /// <summary>
    /// 事件处理器接口
    /// </summary>
    public interface IEventHandler<in TEvent> where TEvent : IEvent
    {
        Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
    }
}
