// =======================================
// 作者：andy.hu
// 文件：IUnitOfWork.cs
// 描述：工作单元接口，支持事务控制
// =======================================

using System;
using System.Threading.Tasks;

namespace Andux.Core.EfTrack
{
    /// <summary>
    /// 工作单元
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// 提交所有更改
        /// </summary>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// 开启事务
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// 提交事务
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// 回滚事务
        /// </summary>
        Task RollbackTransactionAsync();
    }
}
