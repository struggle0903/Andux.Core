using Andux.Core.EventBus.Abstractions;
using Andux.Core.EventBus.Core;
using Andux.Core.EventBus.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;

namespace Andux.Core.EventBus.Extensions
{
    public static class EventBusServiceCollectionExtensions
    {
        public static IServiceCollection AddEventBus(this IServiceCollection services, Action<RabbitMqOptions> configure)
        {
            services.Configure(configure);
            //services.AddSingleton<IEventBus, RabbitMqEventBus>();
            services.AddSingleton<EventBusDispatcher>();
            return services;
        }

        public static IServiceCollection AddEventHandler<TEvent, THandler>(this IServiceCollection services)
            where TEvent : class
            where THandler : class, IEventHandler<TEvent>
        {
            services.AddScoped<IEventHandler<TEvent>, THandler>();
            return services;
        }
    }
}
