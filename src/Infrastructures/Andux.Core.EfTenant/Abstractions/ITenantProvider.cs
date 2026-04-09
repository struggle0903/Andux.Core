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

        /// <summary>
        /// 手动设置租户ID（用于非HTTP场景，如后台任务、消息队列等）
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        void SetTenantId(long tenantId);

        /// <summary>
        /// 清除手动设置的租户ID
        /// </summary>
        void ClearTenantId();
    }
}
