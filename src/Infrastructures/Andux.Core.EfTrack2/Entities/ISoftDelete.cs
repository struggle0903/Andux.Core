// =======================================
// 作者：andy.hu
// 文件：ISoftDelete.cs
// 描述：实体基础接口，定义软删除泛型
// =======================================

namespace Andux.Core.EfTrack
{
    /// <summary>
    /// 软删除标记接口
    /// </summary>
    public interface ISoftDelete
    {
        /// <summary>
        /// 是否已删除
        /// </summary>
        bool IsDeleted { get; set; }
    }
}
