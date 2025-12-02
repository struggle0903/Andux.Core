using System.Collections.Concurrent;

namespace Andux.Core.RabbitMQ.Interfaces
{
    /// <summary>
    /// 租户服务工厂
    /// </summary>
    public interface IRabbitMQTenantServiceFactory
    {
        /// <summary>
        /// 获取租户服务
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        IRabbitMQTenantService GetService(string tenantId);

        /// <summary>
        /// 获取当前所有租户服务
        /// </summary>
        /// <returns></returns>
        ConcurrentDictionary<string, IRabbitMQTenantService> GetAllService();
    }
}
