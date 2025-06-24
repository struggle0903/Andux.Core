using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Andux.Core.Testing.Controllers.Base;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Andux.Core.Testing.Controllers
{
    public class AccountController : ApiBaseController
    {
        /// <summary>
        /// 模拟登录：写入 Cookie 并保存 Claims
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "user123"),
                new Claim(ClaimTypes.Name, "andy"),
                new Claim("projectId", "123"),
                new Claim("id", "111"),
                new Claim("identityType", "101"), // 101 为超管标识，如果是101则不参与ProjectId的过滤
                new Claim(ClaimTypes.NameIdentifier, "123456"),
                new Claim("tenantId", "tenant-001")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSuperSecretKeyForJwtToken123!@#"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "your-app",
                audience: "your-client",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                message = "登录成功",
                token = tokenString
            });
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
