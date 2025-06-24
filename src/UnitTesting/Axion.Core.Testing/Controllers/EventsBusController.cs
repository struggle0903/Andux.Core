using Andux.Core.EventBus;
using Andux.Core.EventBus.Core;
using Andux.Core.Testing.Controllers.Base;
using Andux.Core.Testing.Services;
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
        /// 查询订单（带分页 + 指定导航属性）
        /// </summary>
        [HttpPost("publish")]
        public async Task<IActionResult> PublishAsync()
        {
            await _eventBus.PublishAsync(new UserCreatedEvent { UserId = "u123" });
            return Ok();
        }

    }

    public class CreateUserDto
    {
        public string Email { get; set; } = default!;
    }

}
