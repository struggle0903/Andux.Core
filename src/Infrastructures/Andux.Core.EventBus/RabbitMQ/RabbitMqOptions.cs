using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andux.Core.EventBus.RabbitMQ
{
    public class RabbitMqOptions
    {
        public string HostName { get; set; } = "localhost";
        public string ExchangeName { get; set; } = "eventbus.exchange";
    }
}
