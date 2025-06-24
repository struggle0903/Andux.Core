using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andux.Core.EventBus
{
    public class EventBusOptions
    {
        public string? Provider { get; set; }
        public string? QueueName { get; set; }
    }
}
