using Andux.Core.Common.EventBusDto;
using Andux.Core.EventBus.Events;

namespace Andux.Core.Testing2.Events
{
    /// <summary>
    /// 同步数据事件处理
    /// </summary>
    public class SyncDataEventHandler : IEventHandler<SyncDataEvent>
    {
        private readonly ILogger<SyncDataEventHandler> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger"></param>
        public SyncDataEventHandler(ILogger<SyncDataEventHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(SyncDataEvent @event, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("正在处理同步数据事件 OrderId: {Id}, ProductName: {Name}", @event.Id, @event.Name);
            return Task.CompletedTask;
        }
    }
}
