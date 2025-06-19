using Andux.Core.RabbitMQ.Interfaces;
using Andux.Core.Testing.Entitys;

namespace Andux.Core.Testing.Services
{
    public class OrderProcessingService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<OrderProcessingService> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="services"></param>
        /// <param name="logger"></param>
        public OrderProcessingService(
            IServiceProvider services,
            ILogger<OrderProcessingService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Order Processing Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _services.CreateScope())
                {
                    try
                    {
                        #region 租户消费者
                        var tenantService = scope.ServiceProvider
                            .GetRequiredService<IRabbitMQTenantService>();

                        // 实际消费逻辑
                        tenantService.Consumer.StartConsuming<Order>("andux.test.queue", order =>
                        {
                            _logger.LogInformation(
                                "[Tenant:{TenantId}] Processing order {OrderId}",
                                tenantService.TenantId, order.Id);
                            return Task.CompletedTask;
                        });
                        #endregion

                        #region 直接消费者
                        var consumer = scope.ServiceProvider
                            .GetRequiredService<IRabbitMQConsumer>();
                        consumer.StartConsuming<Order>("andux.test.queue", order =>
                        {
                            _logger.LogInformation(
                                "[Tenant:{TenantId}] Processing order {OrderId}",
                                tenantService.TenantId, order.Id);
                            return Task.CompletedTask;
                        });
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing orders");
                    }
                }

                await Task.Delay(5000, stoppingToken);
            }
        }

    }
}
