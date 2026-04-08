using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Andux.Core.EfTenant.Tenant
{
    /// <summary>
    /// 基于 HttpContext 或 JWT 的租户提供器实现
    /// 从当前请求上下文中解析租户标识
    /// </summary>
    public class DefaultTenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public DefaultTenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 获取当前租户ID
        /// </summary>
        /// <returns>租户ID</returns>
        /// <exception cref="InvalidOperationException">当无法获取租户信息时抛出</exception>
        public long TenantId
        {
            get
            {
                var tenantId = ExtractTenantId();

                // 允许匿名 fallback
                return tenantId ?? 1843473246985555555;
            }
        }

        /// <summary>
        /// 尝试获取当前租户ID，不抛出异常
        /// </summary>
        /// <returns>租户ID，若获取失败则返回 null</returns>
        public long? TryGetTenantId()
        {
            return ExtractTenantId();
        }

        /// <summary>
        /// 从 HttpContext 中提取租户ID
        /// </summary>
        private long? ExtractTenantId()
        {
            // 检查 HttpContext 是否可用
            if (_httpContextAccessor.HttpContext == null)
            {
                return null;
            }

            // 方式1: 从 JWT Claim 中获取
            var tenantIdClaim = _httpContextAccessor.HttpContext.User?.FindFirst("tenantId")?.Value;
            if (!string.IsNullOrEmpty(tenantIdClaim) && long.TryParse(tenantIdClaim, out var claimTenantId))
            {
                return claimTenantId;
            }

            // 方式2: 从请求头中获取（可选扩展）
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var headerTenantId))
            {
                if (long.TryParse(headerTenantId, out var headerId))
                {
                    return headerId;
                }
            }

            // 方式3: 从路由参数中获取（可选扩展）
            if (_httpContextAccessor.HttpContext.GetRouteData().Values.TryGetValue("tenantId", out var routeTenantId))
            {
                if (long.TryParse(routeTenantId?.ToString(), out var routeId))
                {
                    return routeId;
                }
            }

            return null;
        }

    }
}
