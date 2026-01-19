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
        private readonly IRabbitMQPublisher _inner;
        private readonly IRabbitMQConsumer _mqConsume;
        private readonly IRabbitMQConnectionProvider _connectionProvider;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="mqConsume"></param>
        /// <param name="connectionProvider"></param>
        public MqTestController(IRabbitMQPublisher inner, IRabbitMQConsumer mqConsume, IRabbitMQConnectionProvider connectionProvider)
        {
            _inner = inner;
            _mqConsume = mqConsume;
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
        public IActionResult CreateOrder()
        {
            var order = new Order(){ Id = 999 };

            // 直接发布
            //_inner.PublishToQueue("andux.test.queue2", order);
            //_inner.PublishToExchange("andux.iot.gateway", "andux.test.queue2", order);

            // 直接发布广播
            //_inner.PublishTopic("andux.iot.gateway", "andux.topic.queue", order);

            var order2 = new Order() { Id = 10999 };
            _inner.PublishBroadcast("omp.biz.gateway", order2);

            return Accepted();
        }

        [HttpGet("subscribe")]
        public async Task<IActionResult> Subscribe()
        {
            var tcs = new TaskCompletionSource<Order>();

            //_mqConsume.StartConsumingBroadcast<Order>("omp.biz.gateway", "", "omp.biz.log", order =>
            //{
            //    tcs.TrySetResult(order);
            //    return Task.CompletedTask;
            //});

            //_mqConsume.StartConsumingExchange<Order>("omp.biz.gateway", "dddd", "omp.biz.log", order =>
            //{
            //    tcs.TrySetResult(order);
            //    return Task.CompletedTask;
            //});

            #region // 直接发布接收
            //_mqConsume.StartConsuming<Order>("andux.test.queue2", order =>
            //{
            //    tcs.TrySetResult(order);
            //    return Task.CompletedTask;
            //});

            //_mqConsume.StartConsumingExchange<Order>("andux.iot.gateway", "andux.8888.queue", "andux.topic.queue", order =>
            //{
            //    tcs.TrySetResult(order);
            //    return Task.CompletedTask;
            //});
            //_mqConsume.StartConsumingExchange<Order>("andux.iot.gateway", "andux.9999.queue", "andux.topic.queue", order =>
            //{
            //    tcs.TrySetResult(order);
            //    return Task.CompletedTask;
            //});
            #endregion

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
