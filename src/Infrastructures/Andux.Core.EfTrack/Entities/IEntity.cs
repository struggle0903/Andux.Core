// =======================================
// 作者：andy.hu
// 文件：IEntity.cs
// 描述：实体基础接口，定义主键泛型
// =======================================

namespace Andux.Core.EfTrack
{
    /// <summary>
    /// 实体基础接口，所有实体都应实现
    /// </summary>
    /// <typeparam name="TKey">主键类型</typeparam>
    public interface IEntity<TKey>
    {
        TKey Id { get; set; }
    }
}
