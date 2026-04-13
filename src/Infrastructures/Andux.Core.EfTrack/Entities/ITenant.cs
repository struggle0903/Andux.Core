// =======================================
// 作者：andy.hu
// 文件：ITenant.cs
// 描述：实体基础接口，租户数据隔离标识接口
// =======================================

namespace Andux.Core.EfTrack.Entities
{
    /// <summary>
    /// 租户数据隔离标识接口
    /// </summary>
    public interface ITenant
    {
        /// <summary>
        /// 租户id
        /// </summary>
        public long TenantId { get; set; }
    }
}
