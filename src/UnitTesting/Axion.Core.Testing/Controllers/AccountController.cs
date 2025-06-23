using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Andux.Core.Testing.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        /// <summary>
        /// 模拟登录：写入 Cookie 并保存 Claims
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "user123"),
                new Claim(ClaimTypes.Name, "andy"),
                new Claim("projectId", "123"),
                new Claim("id", "111"),
                new Claim("identityType", "101"), // 101 为超管标识，如果是101则不参与ProjectId的过滤
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return Ok(new { message = "登录成功" });
        }

        /// <summary>
        /// 注销登录
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "注销成功" });
        }

        /// <summary>
        /// 获取登录用户信息（需要登录）
        /// </summary>
        [Authorize]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            var username = User.Identity?.Name;
            var projectId = User.Claims.FirstOrDefault(c => c.Type == "ProjectId")?.Value;

            return Ok(new
            {
                Username = username,
                ProjectId = projectId
            });
        }

    }
}
