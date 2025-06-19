using Andux.Core.RabbitMQ.Interfaces;
using Andux.Core.Testing.Entitys;
using Microsoft.AspNetCore.Mvc;

namespace Andux.Core.Testing.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MqTestController : ControllerBase
    {
        private readonly IRabbitMQTenantService _tenantService;
        private readonly IRabbitMQPublisher _inner;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="tenantService"></param>
        /// <param name="inner"></param>
        public MqTestController(IRabbitMQTenantService tenantService, IRabbitMQPublisher inner)
        {
            _tenantService = tenantService;
            _inner = inner;
        }

        [HttpPost]
        public IActionResult CreateOrder([FromBody] Order order)
        {
            // 自动使用当前租户的配置
            _tenantService.Publisher.PublishToQueue("andux.test.queue", order);

            return Accepted();
        }

        [HttpGet("subscribe")]
        public async Task<IActionResult> Subscribe()
        {
            var tcs = new TaskCompletionSource<Order>();

            _tenantService.Consumer.StartConsuming<Order>("andux.test.queue", order =>
            {
                tcs.TrySetResult(order);
                return Task.CompletedTask;
            });

            // 等待5秒接收消息
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(5000));

            if (completedTask == tcs.Task)
            {
                return Ok(tcs.Task.Result);
            }

            return NoContent();
        }

    }
}
