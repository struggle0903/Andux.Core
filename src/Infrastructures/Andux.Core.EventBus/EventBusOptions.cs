using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andux.Core.EventBus
{
    public class EventBusOptions
    {
        public string Provider { get; set; } = "InMemory"; // or "RabbitMQ", "Kafka"
        public string? QueueName { get; set; }
        public bool EnableRetry { get; set; } = false;
        public int RetryCount { get; set; } = 3;
    }
}
