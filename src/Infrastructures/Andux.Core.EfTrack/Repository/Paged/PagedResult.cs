// =======================================
// 作者：andy.hu
// 文件：PagedResult.cs
// 描述：分页结果对象
// =======================================

using System.Collections.Generic;

namespace Andux.Core.EfTrack.Repository.Paged
{
    /// <summary>
    /// 分页结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// 当前页
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每页多少条
        /// </summary>
        public int Limit { get; set; } = 15;

        /// <summary>
        /// 总数量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 数据集合
        /// </summary>
        public List<T> Items { get; set; } = [];
    }
}
