using Andux.Core.Common.EventBusDto;
using Andux.Core.EventBus.Events;

namespace Andux.Core.Testing2.Events
{
    /// <summary>
    /// 分布式订单事件处理
    /// </summary>
    public class AddOrderEventHandler : IEventHandler<AddOrderEvent>
    {
        private readonly ILogger<AddOrderEventHandler> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger"></param>
        public AddOrderEventHandler(ILogger<AddOrderEventHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(AddOrderEvent @event, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("正在处理分布式订单事件 OrderId: {OrderId}, ProductName: {ProductName}", @event.OrderId, @event.ProductName);
            return Task.CompletedTask;
        }
    }
}
