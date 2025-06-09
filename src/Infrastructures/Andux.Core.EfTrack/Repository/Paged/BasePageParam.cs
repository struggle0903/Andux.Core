// =======================================
// 作者：andy.hu
// 文件：BasePageParam.cs
// 描述：基础分页参数
// =======================================

namespace Andux.Core.EfTrack.Repository.Paged
{
    /// <summary>
    /// 基础分页参数
    /// </summary>
    public class BasePageParam
    {
        /// <summary>
        /// 当前页
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每页多少条
        /// </summary>
        public int Limit { get; set; } = 15;
    }
}
