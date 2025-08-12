using Andux.Admin.Application.Account;
using Andux.Admin.Application.Account.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Andux.Admin.Host.Controllers
{
    /// <summary>
    /// 账户相关
    /// </summary>
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="accountService"></param>
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var device = Request.Headers["User-Agent"].ToString();

            var result = await _accountService.LoginAsync(request, ip, device);
            return Ok(result);
        }

        /// <summary>
        /// 注销
        /// </summary>
        /// <returns></returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var sessionId = User.FindFirst("jti")?.Value;
            if (sessionId == null) return BadRequest("无效会话");

            await _accountService.LogoutAsync(sessionId);
            return Ok();
        }

    }
}
