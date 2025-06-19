using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andux.Core.RabbitMQ.Models
{
    /// <summary>
    /// RabbitMQ 事件基类
    /// </summary>
    public abstract class RabbitMQEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    }
}
