using Andux.Core.RabbitMQ.Models;
using RabbitMQ.Client;

namespace Andux.Core.RabbitMQ.Interfaces
{
    /// <summary>
    /// RabbitMQ连接提供者(支持多租户)
    /// </summary>
    public interface IRabbitMQConnectionProvider : IDisposable
    {
        /// <summary>
        /// 获取默认连接
        /// </summary>
        IConnection GetConnection();

        /// <summary>
        /// 获取租户专属连接
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        IConnection GetTenantConnection(string tenantId);

        /// <summary>
        /// 创建通道
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        IModel CreateChannel(string tenantId = null);

        /// <summary>
        /// 注册租户配置
        /// </summary>
        /// <param name="options"></param>
        void RegisterTenant(TenantOptions options);
    }
}
