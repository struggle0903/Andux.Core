using Andux.Core.EventBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andux.Core.EventBus.Core
{
    public class EventBusDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        
        public EventBusDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task DispatchAsync<TEvent>(TEvent @event) where TEvent : class
        {
            //var handlers = _serviceProvider.GetServices<IEventHandler<TEvent>>();
            //foreach (var handler in handlers)
            //{
            //    await handler.HandleAsync(@event);
            //}
        }
    }
}
