// =======================================
// 作者：andy.hu
// 文件：IProject.cs
// 描述：实体基础接口，项目数据隔离标识接口
// =======================================

namespace Andux.Core.EfTrack.Entities
{
    /// <summary>
    /// 项目数据隔离标识接口
    /// </summary>
    public interface IProject
    {
        long? ProjectId { get; set; }
    }
}
