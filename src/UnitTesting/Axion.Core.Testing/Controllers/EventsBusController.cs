using Andux.Core.Common.EventBusDto;
using Andux.Core.EventBus.Events;
using Andux.Core.Testing.Controllers.Base;
using Andux.Core.Testing.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Andux.Core.Testing.Controllers
{
    [AllowAnonymous]
    public class EventsBusController : ApiBaseController
    {
        private readonly IEventBus _eventBus;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventBus"></param>
        public EventsBusController(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        /// <summary>
        /// 内部程序创建用户，并发布用户创建事件
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("create")]
        public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserDto dto)
        {
            // 模拟创建用户成功
            var userId = Guid.NewGuid().ToString();

            try
            {
                // 发布事件
                await _eventBus.PublishAsync(new UserCreatedEvent
                {
                    UserId = userId,
                    Email = dto.Email
                });
            }
            catch(Exception ex)
            {

            }

            return Ok(new { userId, message = "User created and event published." });
        }

        /// <summary>
        /// 内部程序创建用户，并发布日志创建事件
        /// </summary>
        /// <returns></returns>
        [HttpPost("addLog")]
        public async Task<IActionResult> AddLoggerAsync()
        {
            try
            {
                // 发布事件
                await _eventBus.PublishAsync(new AddLoggerEvent
                {
                    Type = "test",
                    Message = Guid.NewGuid().ToString()
                });
            }
            catch (Exception ex)
            {

            }

            return Ok(new { message = "log add and event published." });
        }

        /// <summary>
        /// 事件发布者
        /// 分布式程序创建订单，并发布创建订单事件
        /// </summary>
        /// <returns></returns>
        [HttpPost("addOrder")]
        public async Task<IActionResult> CreateOrderAsync()
        {
            try
            {
                // 发布事件
                await _eventBus.PublishAsync(new AddOrderEvent
                {
                    OrderId = Guid.NewGuid(),
                    ProductName = "测试商品",
                    Price = 20,
                    Count = 2
                });
            }
            catch (Exception ex)
            {

            }

            return Ok("分布式程序创建订单，并发布创建订单事件");
        }

        /// <summary>
        /// 事件发布者
        /// 分布式程序发布同步数据事件
        /// </summary>
        /// <returns></returns>
        [HttpPost("syncData")]
        public async Task<IActionResult> SyncDataAsync()
        {
            try
            {
                // 发布事件
                await _eventBus.PublishAsync(new SyncDataEvent
                {
                    Id = Guid.NewGuid(),
                    Name = "张三",
                });
            }
            catch (Exception ex)
            {

            }

            return Ok("分布式程序创建订单，并发布创建订单事件");
        }

    }

    public class CreateUserDto
    {
        public string Email { get; set; } = null!;
    }

}
