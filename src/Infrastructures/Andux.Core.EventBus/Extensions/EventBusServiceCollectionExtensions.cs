using Andux.Core.EventBus.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Andux.Core.EventBus.Extensions
{
    public static class EventBusServiceCollectionExtensions
    {
        public static IServiceCollection UseAnduxEventBus(this IServiceCollection services)
        {
            services.AddSingleton<IEventBus, InMemoryEventBus>();
            return services;
        }
    }
}
