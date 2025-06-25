using Andux.Core.RabbitMQ.Interfaces;
using Andux.Core.Testing.Controllers.Base;
using Andux.Core.Testing.Entitys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Andux.Core.Testing.Controllers
{
    [AllowAnonymous]
    public class MqTestController : ApiBaseController
    {
        private readonly IRabbitMQTenantService _tenantService;
        private readonly IRabbitMQPublisher _inner;
        private readonly IRabbitMQConnectionProvider _connectionProvider;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="tenantService"></param>
        /// <param name="inner"></param>
        /// <param name="connectionProvider"></param>
        public MqTestController(IRabbitMQTenantService tenantService, IRabbitMQPublisher inner, IRabbitMQConnectionProvider connectionProvider)
        {
            _tenantService = tenantService;
            _inner = inner;
            _connectionProvider = connectionProvider;
        }

        /// <summary>
        /// 获取所有连接对象
        /// </summary>
        /// <returns></returns>
        [HttpGet("getConnections")]
        public IActionResult GetConnections()
        {
            // 获取所有连接对象
            var allConnections = _connectionProvider.GetAllConnections();
            return Ok(allConnections);
        }

        /// <summary>
        /// 删除指定连接对象
        /// </summary>
        /// <returns></returns>
        [HttpGet("del")]
        public IActionResult GetConnections(string tenantId)
        {
            // 获取所有连接对象
            _connectionProvider.RemoveConnection(tenantId);
            return Ok("已删除");
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
