using Andux.Core.Extensions;
using Andux.Core.RabbitMQ.Interfaces;
using Andux.Core.Testing.Entitys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Andux.Core.Testing.Controllers
{
    [AllowAnonymous]
    public class MqTenantTestController : Controller
    {
        private readonly IRabbitMQPublisher _inner;
        private readonly IRabbitMQConsumer _mqConsume;
        private readonly IRabbitMQConnectionProvider _connectionProvider;
        private readonly IRabbitMQTenantServiceFactory _tenantServiceFactory;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="mqConsume"></param>
        /// <param name="connectionProvider"></param>
        /// <param name="tenantServiceFactory"></param>
        public MqTenantTestController(
            IRabbitMQPublisher inner, 
            IRabbitMQConsumer mqConsume, 
            IRabbitMQConnectionProvider connectionProvider,
            IRabbitMQTenantServiceFactory tenantServiceFactory)
        {
            _inner = inner;
            _mqConsume = mqConsume;
            _connectionProvider = connectionProvider;
            _tenantServiceFactory = tenantServiceFactory;
        }

        /// <summary>
        /// 获取所有连接对象
        /// </summary>
        /// <returns></returns>
        [HttpGet("getConnections")]
        public IActionResult GetConnections()
        {
            // 获取所有连接对象(如果对象中没有，需要初始化)
            var allConnections = _connectionProvider.GetAllConnections();
            return Ok(allConnections);
        }

        /// <summary>
        /// 获取所有已创建的租户服务
        /// </summary>
        /// <returns></returns>
        [HttpGet("getTenantServices")]
        public IActionResult GetTenantServices()
        {
            // 获取所有租户服务
            var allTenantService = _tenantServiceFactory.GetAllService();
            return Ok(allTenantService);
        }

        /// <summary>
        /// 删除指定连接对象
        /// </summary>
        /// <returns></returns>
        [HttpGet("delConnection")]
        public IActionResult GetConnections(string tenantId)
        {
            // 获取所有连接对象
            _connectionProvider.RemoveConnection(tenantId);
            return Ok("已删除");
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <returns></returns>
        [HttpPost("publish")]
        public IActionResult CreateOrder()
        {
            //var order = new Order() { Id = 999 };
            //_inner.PublishToQueue("123456", order);

            //var order2 = new Order() { Id = 101022 };
            //_inner.PublishToExchange("omp.biz.gateway", "omp.biz.log", order2);

            #region 租户工厂发布消息

            var aOrder = new Order() { Id = 111 };

            //var andyTenant = _tenantServiceFactory.GetService("andy");
            ////andyTenant.Publisher.PublishTopic("andux2.iot.gateway", "andux.test.rkey", aOrder);
            //andyTenant.Publisher.PublishTopic("andux2.iot.gateway", "andux.test.rkey", "1232222");

            var yjxfhhTenant = _tenantServiceFactory.GetService("yjxfhh");
            yjxfhhTenant.Publisher.PublishTopic("andux2.iot.gateway", "andux.test.rkey", aOrder);

            //var hOrder = new Order() { Id = 222 };
            //var huTenant = _tenantServiceFactory.GetService("hu");
            //huTenant.Publisher.PublishTopic("andux.iot.gateway", "andux.test.rkey2", hOrder);

            //var pOrder = new Order() { Id = 3333 };
            //var proTenant = _tenantServiceFactory.GetService("pro");
            //proTenant.Publisher.PublishTopic("andux.iot.gateway", "andux.test.rkey", pOrder);

            #endregion

            return Accepted();
        }

        /// <summary>
        /// 订阅/消费
        /// </summary>
        /// <returns></returns>
        [HttpGet("subscribe")]
        public async Task<IActionResult> Subscribe()
        {
            var tcs = new TaskCompletionSource<Order>();

            //_mqConsume.StartConsuming<Order>("123456", order =>
            //{
            //    tcs.TrySetResult(order);
            //    return Task.CompletedTask;
            //});

            //_mqConsume.StartConsumingExchange<Order>("omp.biz.gateway", "omp.biz.log.queue", "omp.biz.log", order =>
            //{
            //    tcs.TrySetResult(order);
            //    return Task.CompletedTask;
            //});

            var yjxfhhTenant = _tenantServiceFactory.GetService("yjxfhh");
            yjxfhhTenant.Consumer.StartConsumingTopic<string>("andux2.iot.gateway", "andux.test.rkey", "biz.andux.test.queue", order =>
            {
                //tcs.TrySetResult(order);
                return Task.CompletedTask;
            });

            yjxfhhTenant.Consumer.StartConsumingTopic<Order>("andux2.iot.gateway", "andux.test.rkey", "biz.andux.test2.queue", order =>
            {
                //tcs.TrySetResult(order);
                return Task.CompletedTask;
            });

            #region 租户工厂消费

            //var andyTenant = _tenantServiceFactory.GetService("andy");
            //andyTenant.Consumer.StartConsumingTopic<Order>("andux2.iot.gateway", "andux.test.rkey", "andy.andux.test.queue", order =>
            //{
            //    tcs.TrySetResult(order);
            //    return Task.CompletedTask;
            //});

            //andyTenant.Consumer.StartConsumingTopic<Order>("andux2.iot.gateway", "andux.test.rkey", "andy.andux.test2.queue", order =>
            //{
            //    tcs.TrySetResult(order);
            //    return Task.CompletedTask;
            //});

            //var huTenant = _tenantServiceFactory.GetService("hu");
            //huTenant.Consumer.StartConsumingTopic<Order>("andux.iot.gateway", "andux.test.rkey2", "andy.andux.test.queue", order =>
            //{
            //    tcs.TrySetResult(order);
            //    return Task.CompletedTask;
            //});

            //huTenant.Consumer.StartConsumingTopic<Order>("andux.iot.gateway", "andux.test.rkey2", "andy.andux.test2.queue", order =>
            //{
            //    tcs.TrySetResult(order);
            //    return Task.CompletedTask;
            //});

            //var proTenant = _tenantServiceFactory.GetService("pro");
            //proTenant.Consumer.StartConsumingTopic<Order>("andux.iot.gateway", "andux.test.rkey", "andy.andux.test.queue", order =>
            //{
            //    tcs.TrySetResult(order);
            //    return Task.CompletedTask;
            //});

            //proTenant.Consumer.StartConsumingTopic<Order>("andux.iot.gateway", "andux.test.rkey", "andy.andux.test2.queue", order =>
            //{
            //    tcs.TrySetResult(order);
            //    return Task.CompletedTask;
            //});

            #endregion

            // 等待5秒接收消息
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(2000));

            if (completedTask == tcs.Task)
            {
                return Ok(tcs.Task.Result);
            }

            return NoContent();
        }
    }
}
