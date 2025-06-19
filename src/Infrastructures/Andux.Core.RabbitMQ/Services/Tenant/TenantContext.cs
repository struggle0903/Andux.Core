using Andux.Core.RabbitMQ.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andux.Core.RabbitMQ.Services.Tenant
{
    /// <summary>
    /// 
    /// </summary>
    public class TenantContext : ITenantContext
    {
        public string TenantId { get; }

        public TenantContext(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
                throw new ArgumentException("Tenant ID cannot be null or empty", nameof(tenantId));

            TenantId = tenantId;
        }
    }
}
