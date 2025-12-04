using Andux.Core.RabbitMQ.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Andux.Core.RabbitMQ.Services.Tenant
{
    /// <summary>
    /// RabbitMQ多租户服务工厂
    /// </summary>
    public class RabbitMQTenantServiceFactory : IRabbitMQTenantServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 租户服务缓存
        /// </summary>
        private readonly ConcurrentDictionary<string, IRabbitMQTenantService> _cache = new();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider"></param>
        public RabbitMQTenantServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 获取租户服务
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public IRabbitMQTenantService GetService(string tenantId)
        {
            return _cache.GetOrAdd(tenantId, id => CreateRabbitMQTenantService(_serviceProvider, id));
        }

        /// <summary>
        /// 获取当前所有租户服务
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<string, IRabbitMQTenantService> GetAllService()
        {
            return _cache;
        }

        #region 私有方法

        /// <summary>
        /// 创建租户服务
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        private static IRabbitMQTenantService CreateRabbitMQTenantService(
            IServiceProvider sp, string tenantId)
        {
            var connectionProvider = sp.GetRequiredService<IRabbitMQConnectionProvider>();
            var publisher = sp.GetRequiredService<IRabbitMQPublisher>();
            var consumer = sp.GetRequiredService<IRabbitMQConsumer>();

            // 获取租户配置选项
            var tenantOptions = connectionProvider.GetMQTenantOptions(tenantId);

            return new RabbitMQTenantService(
                tenantId,
                tenantOptions,
                connectionProvider,
                publisher,
                consumer);
        }
        #endregion
    }
}
