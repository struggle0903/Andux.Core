using Andux.Core.EventBus.Events;

namespace Andux.Core.Common.EventBusDto
{
    /// <summary>
    /// 新增订单事件
    /// </summary>
    public class AddOrderEvent : IEvent
    {
        public Guid OrderId { get; set; }
        public string ProductName { get; set; } = null!;
        public double Price { get; set; }
        public int Count { get; set; }
    }
}
