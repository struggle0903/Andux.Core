using Andux.Core.RabbitMQ.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Andux.Core.RabbitMQ.Models;
using Andux.Core.RabbitMQ.Services.Connection;
using Andux.Core.RabbitMQ.Services.Consumers;
using Andux.Core.RabbitMQ.Services.Publishers;
using Andux.Core.RabbitMQ.Services.Tenant;

namespace Andux.Core.RabbitMQ.Extensions
{
    /// <summary>
    /// 服务集合扩展方法
    /// </summary>
    public static class RabbitMQServiceExtensions
    {
        /// <summary>
        /// Andux.Core.RabbitMQ 服务注册扩展方法
        /// 普通模式（支持IRabbitMQConnectionProvider，IRabbitMQPublisher， IRabbitMQConsumer）
        /// 只能注册单个MQ服务地址的单个用户连接
        /// </summary>
        /// <param name="services"></param>
        /// <param name="rabbitMQConfig"></param>
        /// <returns></returns>
        public static IServiceCollection UseAnduxRabbitMQServices(this IServiceCollection services, RabbitMQOptions rabbitMQConfig)
        {
            services.AddSingleton(rabbitMQConfig);

            // 注册连接提供者
            services.AddSingleton<IRabbitMQConnectionProvider>(sp =>
            {
                var provider = new RabbitMQConnectionProvider(rabbitMQConfig);
                return provider;
            });

            services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();
            services.AddScoped<IRabbitMQPublisher, RabbitMQPublisher>();
            services.AddSingleton<IRabbitMQConsumer, RabbitMQConsumer>();

            return services;
        }

        /// <summary>
        /// Andux.Core.RabbitMQ 服务注册扩展方法
        /// 多租户模式（支持IRabbitMQConnectionProvider，IRabbitMQPublisher， IRabbitMQConsumer，IRabbitMQTenantService）
        /// 支持同时注册多个连接，和管理连接 （使用此模式后所有的exchangeName，routingKey以及queueName前面都会自动加上 $"{tenantId}."）
        /// </summary>
        /// <param name="services"></param>
        /// <param name="tenantConfigs"></param>
        /// <returns></returns>
        public static IServiceCollection UseAnduxTenantRabbitMQServices(this IServiceCollection services, List<RabbitMQTenantOptions> tenantConfigs)
        {
            if (tenantConfigs.Count <= 0)
                throw new AbandonedMutexException("无租户数据，注册并使用AnduxRabbitMQ失败");

            services.AddSingleton(tenantConfigs);

            // 默认取第一天数据为默认租户
            var defaultTenant = tenantConfigs.First();
            var rabbitMQConfig = new RabbitMQOptions()
            {
                Host = defaultTenant.Host,
                Port = defaultTenant.Port,
                UserName = defaultTenant.UserName,
                Password = defaultTenant.Password,
                VirtualHost = defaultTenant.VirtualHost,
                NetworkRecoveryInterval = defaultTenant.NetworkRecoveryInterval,
            };

            // 注册连接提供者（支持多租户）
            services.AddSingleton<IRabbitMQConnectionProvider>(sp =>
            {
                var provider = new RabbitMQConnectionProvider(rabbitMQConfig);
                (tenantConfigs).ForEach(provider.RegisterTenant);
                return provider;
            });

            services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();
            services.AddSingleton<IRabbitMQConsumer, RabbitMQConsumer>();

            // 租户服务工厂
            services.AddSingleton<IRabbitMQTenantServiceFactory, RabbitMQTenantServiceFactory>();

            return services;
        }
    }
}
