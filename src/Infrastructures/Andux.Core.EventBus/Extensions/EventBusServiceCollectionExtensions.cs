using Andux.Core.EventBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Andux.Core.EventBus.Extensions
{
    public static class EventBusServiceCollectionExtensions
    {
        public static IServiceCollection UseAnduxEventBus(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetSection("EventBus").Get<EventBusOptions>()!;
            return options.Provider switch
            {
                "InMemory" => services.AddSingleton<IEventBus, InMemoryEventBus>(),
                "RabbitMQ" => services.AddSingleton<IEventBus, RabbitMqEventBus>(),
                _ => throw new InvalidOperationException("Unknown provider: " + options.Provider)
            };

            services.AddSingleton<IEventBus, InMemoryEventBus>();
            return services;
        }
    }
}
