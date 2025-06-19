using Andux.Core.RabbitMQ.Interfaces;

namespace Andux.Core.RabbitMQ.Services.Tenant
{
    /// <summary>
    /// 租户上下文实现
    /// </summary>
    public class TenantContext : ITenantContext
    {
        public string TenantId { get; }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="tenantId"></param>
        public TenantContext(string tenantId)
        {
            //if (string.IsNullOrWhiteSpace(tenantId))
            //    throw new ArgumentException("Tenant ID 不能为null或空", nameof(tenantId));

            TenantId = tenantId;
        }
    }
}
