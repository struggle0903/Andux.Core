using Andux.Core.RabbitMQ.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Andux.Core.RabbitMQ.Models;
using Andux.Core.RabbitMQ.Services.Connection;
using Andux.Core.RabbitMQ.Services.Consumers;
using Andux.Core.RabbitMQ.Services.Publishers;
using Andux.Core.RabbitMQ.Services.Tenant;
using Microsoft.Extensions.Configuration;

namespace Andux.Core.RabbitMQ.Extensions
{
    /// <summary>
    /// 服务集合扩展方法
    /// </summary>
    public static class RabbitMQServiceExtensions
    {
        /// <summary>
        /// Andux.Core.RabbitMQ 服务注册扩展方法
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="tenantConfigs"></param>
        /// <returns></returns>
        public static IServiceCollection UseAnduxRabbitMQServices(this IServiceCollection services,
            IConfiguration configuration, List<TenantOptions> tenantConfigs = null)
        {
            // 1. 注册配置
            var rabbitMQConfig = new RabbitMQOptions
            {
                HostName = configuration.GetValue("RabbitMQ:Host", "localhost"),
                Port = configuration.GetValue("RabbitMQ:Port", 5672),
                UserName = configuration.GetValue("RabbitMQ:Username", "guest"),
                Password = configuration.GetValue("RabbitMQ:Password", "guest"),
                ClientProvidedName = configuration.GetValue("RabbitMQ:ClientProvidedName", "Andux.Core.RabbitMQ"),
                VirtualHost = configuration.GetValue("RabbitMQ:VirtualHost", "/"),
                AutomaticRecoveryEnabled = configuration.GetValue("RabbitMQ:AutomaticRecoveryEnabled", false),
                NetworkRecoveryInterval = configuration.GetValue("RabbitMQ:NetworkRecoveryInterval", 10)
            };

            services.AddSingleton(rabbitMQConfig);

            // 2. 注册租户上下文（作用域）
            services.AddScoped<ITenantContext>(sp => new TenantContext("root"));

            // 3. 注册RabbitMQ核心服务
            services.AddSingleton<IRabbitMQConnectionProvider>(sp =>
            {
                var provider = new RabbitMQConnectionProvider(rabbitMQConfig);
                (tenantConfigs ?? []).ForEach(provider.RegisterTenant);
                return provider;
            });

            services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();
            services.AddSingleton<IRabbitMQConsumer, RabbitMQConsumer>();

            // 4. 注册租户服务（作用域）
            services.AddScoped<IRabbitMQTenantService>(sp =>
            {
                var tenantContext = sp.GetRequiredService<ITenantContext>();
                var connectionProvider = sp.GetRequiredService<IRabbitMQConnectionProvider>();
                var publisher = sp.GetRequiredService<IRabbitMQPublisher>();
                var consumer = sp.GetRequiredService<IRabbitMQConsumer>();

                return new RabbitMQTenantService(
                    tenantContext.TenantId,
                    connectionProvider,
                    publisher,
                    consumer);
            });

            return services;
        }

    }
}
