// =======================================
// 作者：andy.hu
// 文件：BaseEntity.cs
// 描述：抽象实体基类，统一实现主键 + 审计字段
// =======================================

using System;

namespace Andux.Core.EfTrack
{
    /// <summary>
    /// 抽象实体基类，包含主键、创建时间、更新时间、创建人、修改人字段
    /// </summary>
    /// <typeparam name="TKey">主键类型</typeparam>
    public abstract class BaseEntity<TKey> : IEntity<TKey>, IAuditedEntity, ISoftDelete
    {
        public TKey Id { get; set; } = default!;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? UpdatedTime { get; set; }

        /// <summary>
        /// 创建人（用户标识）
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 修改人（用户标识）
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// 软删除
        /// </summary>
        public bool IsDeleted { get; set; } = false;
    }
}
