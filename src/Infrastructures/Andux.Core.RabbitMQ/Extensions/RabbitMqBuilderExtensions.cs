using Andux.Core.RabbitMQ.Options;
using Andux.Core.RabbitMQ.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Andux.Core.RabbitMQ.Extensions
{
    public static class RabbitMqBuilderExtensions
    {
        /// <summary>
        /// 将 RabbitMQ 服务添加到 DI 容器
        /// </summary>
        public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            // 从配置中读取 LoggingOptions 节点
            var options = configuration.GetSection("RabbitMQ").Get<RabbitMqOptions>() ?? new RabbitMqOptions();

            services.Configure<RabbitMqOptions>(options);
            services.AddSingleton<IRabbitMqService, RabbitMqService>();
            return services;
        }
    }
}
