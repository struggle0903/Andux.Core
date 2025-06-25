using Andux.Core.EventBus.Events;

namespace Andux.Core.Common.EventBusDto
{
    /// <summary>
    /// 同步数据事件
    /// </summary>
    public class SyncDataEvent : IEvent
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
