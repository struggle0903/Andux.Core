using Andux.Core.EventBus;

namespace Andux.Core.Testing.Services
{
    public class UserCreatedEvent : IEvent
    {
        public string UserId { get; set; } = default!;
        public string Email { get; set; } = default!;
    }

    public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
    {
        private readonly ILogger<UserCreatedEventHandler> _logger;

        public UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(UserCreatedEvent @event, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Handling UserCreatedEvent for user: {UserId}", @event.UserId);
            // TODO: 例如发送欢迎邮件等业务逻辑
            return Task.CompletedTask;
        }
    }
}
