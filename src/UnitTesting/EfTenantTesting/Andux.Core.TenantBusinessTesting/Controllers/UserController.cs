using Andux.Core.TenantTesting.Application.Services;
using Andux.Core.TenantTesting.Application.Services.Request;
using Andux.Core.TenantTesting.Controllers.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Andux.Core.TenantTesting.Controllers
{
    /// <summary>
    /// 用户管理
    /// </summary>
    [ApiController]
    [Route("api/users")]
    [Authorize]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="service"></param>
        public UserController(IUserService service)
        {
            _service = service;
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            var id = await _service.CreateAsync(request.Name);

            return Ok(new { Id = id });
        }

        /// <summary>
        /// 获取用户
        /// </summary>
        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _service.GetAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// 查询用户列表
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] string? keyword)
        {
            var list = await _service.GetListAsync(keyword);

            return Ok(list);
        }

        /// <summary>
        /// 修改用户
        /// </summary>
        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateUserRequest request)
        {
            var success = await _service.UpdateAsync(id, request.Name);

            if (!success)
                return NotFound();

            return Ok();
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var success = await _service.DeleteAsync(id);

            if (!success)
                return NotFound();

            return Ok();
        }

    }
}
