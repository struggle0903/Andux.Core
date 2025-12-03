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
        /// 获取mq配置选项
        /// </summary>
        /// <returns></returns>
        RabbitMQOptions GetMQOptions();

        /// <summary>
        /// 获取指定租户mq配置
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        RabbitMQTenantOptions GetMQTenantOptions(string tenantId);

        /// <summary>
        /// 获取默认连接
        /// </summary>
        IConnection GetConnection();

        /// <summary>
        /// 创建通道
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        IModel CreateChannel(string? tenantId = null);

        /// <summary>
        /// 获取当前所有活跃连接对象
        /// </summary>
        /// <returns>字典集合（Key: 租户ID，Value: RabbitMQ连接）</returns>
        IReadOnlyDictionary<string, IConnection> GetAllConnections();

        /// <summary>
        /// 删除指定连接对象
        /// </summary>
        /// <param name="tenantId"></param>
        void RemoveConnection(string tenantId);

        #region 租户相关

        /// <summary>
        /// 获取租户专属连接
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        IConnection GetTenantConnection(string? tenantId);

        /// <summary>
        /// 注册租户配置
        /// </summary>
        /// <param name="options"></param>
        void RegisterTenant(RabbitMQTenantOptions options);

        #endregion
    }
}
