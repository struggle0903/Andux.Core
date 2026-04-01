// =======================================
// 作者：andy.hu
// 文件：ITenantStore.cs
// 描述：租户信息存储
// =======================================

using Andux.Core.EfTenant.Tenant;

namespace Andux.Core.EfTenant
{
    /// <summary>
    /// 租户信息存储
    /// </summary>
    public interface ITenantStore
    {
        /// <summary>
        /// 获取租户数据库配置
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        Task<TenantDbConfig> GetAsync(long tenantId);

        /// <summary>
        /// 获取租户连接字符串
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        string GetConnectionString(long tenantId);
    }
}
