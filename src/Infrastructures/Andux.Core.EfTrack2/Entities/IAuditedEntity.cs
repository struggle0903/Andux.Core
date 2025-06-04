// =======================================
// 作者：andy.hu
// 文件：IAuditedEntity.cs
// 描述：审计字段接口（创建时间、修改时间、创建人、修改人）
// =======================================

using System;

namespace Andux.Core.EfTrack
{
    /// <summary>
    /// 审计字段接口
    /// </summary>
    public interface IAuditedEntity
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        DateTime CreatedTime { get; set; }

        /// <summary>
        /// 创建人（用户标识）
        /// </summary>
        string? CreatedBy { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        DateTime? UpdatedTime { get; set; }

        /// <summary>
        /// 修改人（用户标识）
        /// </summary>
        string? UpdatedBy { get; set; }
    }
}
