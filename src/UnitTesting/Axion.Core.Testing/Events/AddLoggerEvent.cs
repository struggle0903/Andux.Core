using Andux.Core.EventBus.Events;

namespace Andux.Core.Testing.Events
{
    /// <summary>
    /// 新增日志事件
    /// </summary>
    public class AddLoggerEvent : IEvent
    {
        public string Type { get; set; } = null!;
        public string Message { get; set; } = null!;
    }

    /// <summary>
    /// 用户创建事件处理器
    /// </summary>
    public class AddLoggerEventHandler : IEventHandler<AddLoggerEvent>
    {
        private readonly ILogger<AddLoggerEventHandler> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger"></param>
        public AddLoggerEventHandler(ILogger<AddLoggerEventHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(AddLoggerEvent @event, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("正在处理日志事件 Type: {Type}, Message: {Message}", @event.Type, @event.Message);

            return Task.CompletedTask;
        }
    }
}
