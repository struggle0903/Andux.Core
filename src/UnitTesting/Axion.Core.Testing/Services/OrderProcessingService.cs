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
            _logger.LogInformation("订单处理服务正在启动.");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _services.CreateScope())
                {
                    var consumer = scope.ServiceProvider.GetRequiredService<IRabbitMQConsumer>();
                    var tenantService = scope.ServiceProvider.GetRequiredService<IRabbitMQTenantService>();

                    try
                    {
                        // 先停止可能存在的旧消费者
                        consumer.StopConsuming("andux.test.queue");

                        #region 直接消费者
                        consumer.StartConsuming<Order>("andux.test.queue", order =>
                        {
                            _logger.LogInformation("进入直接消费消息： {OrderId}", order.Id);
                            return Task.CompletedTask;
                        });
                        #endregion

                        #region 消费指定租户消息
                        consumer.StartConsuming<Order>("sfm", "andux.test.queue", order =>
                        {
                            _logger.LogInformation("进入消费指定租户消息sfm： {OrderId}", order.Id);
                            return Task.CompletedTask;
                        });
                        consumer.StartConsuming<Order>("bsb", "andux.test.queue", order =>
                        {
                            _logger.LogInformation("进入消费指定租户消息bsb： {OrderId}", order.Id);
                            return Task.CompletedTask;
                        });
                        #endregion

                        #region 租户消费者

                        // 实际消费逻辑
                        tenantService.Consumer.StartConsuming<Order>("andux.test.queue", order =>
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
                        _logger.LogError(ex, "处理订单时出错");
                        await Task.Delay(1000, stoppingToken); // 出错后短暂等待
                    }
                }

                await Task.Delay(5000, stoppingToken);
            }
        }

    }
}
