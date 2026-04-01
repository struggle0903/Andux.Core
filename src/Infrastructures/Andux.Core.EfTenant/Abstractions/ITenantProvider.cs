// =======================================
// 作者：andy.hu
// 文件：ITenantProvider.cs
// 描述：租户提供器接口定义
// =======================================

namespace Andux.Core.EfTenant
{
    /// <summary>
    /// 租户提供器接口定义
    /// </summary>
    public interface ITenantProvider
    {
        /// <summary>
        /// 当前租户ID
        /// </summary>
        long TenantId { get; }

        /// <summary>
        /// 尝试获取当前租户ID，不抛出异常
        /// </summary>
        long? TryGetTenantId() => null; // 默认实现
    }
}
