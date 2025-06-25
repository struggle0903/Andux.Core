using Andux.Core.EventBus.Events;

namespace Andux.Core.Testing.Events
{
    /// <summary>
    /// 用户创建事件
    /// </summary>
    public class UserCreatedEvent : IEvent
    {
        public string UserId { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    /// <summary>
    /// 用户创建事件处理器
    /// </summary>
    public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
    {
        private readonly ILogger<UserCreatedEventHandler> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger"></param>
        public UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(UserCreatedEvent @event, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("正在处理用户创建事件 user: {UserId}", @event.UserId);

            // TODO: 例如发送欢迎邮件等业务逻辑

            return Task.CompletedTask;
        }
    }
}
