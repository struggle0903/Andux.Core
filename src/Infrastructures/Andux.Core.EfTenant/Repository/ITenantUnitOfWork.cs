// =======================================
// 作者：andy.hu
// 文件：IUnitOfWork.cs
// 描述：工作单元接口，支持事务控制
// =======================================

using Microsoft.EntityFrameworkCore;

namespace Andux.Core.EfTenant
{
    /// <summary>
    /// 工作单元
    /// </summary>
    public interface ITenantUnitOfWork : IDisposable
    {
        /// <summary>
        /// 获取当前 DbContext
        /// </summary>
        DbContext GetDbContext();

        /// <summary>
        /// 保存更改
        /// </summary>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// 开始事务
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

        /// <summary>
        /// 刷新 DbContext（强制重新创建）
        /// </summary>
        void RefreshContext();
    }
}
