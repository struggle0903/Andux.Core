// =======================================
// 作者：andy.hu
// 文件：UnitOfWork.cs
// 描述：工作单元实现，支持事务控制
// =======================================

using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Andux.Core.EfTrack
{
    /// <summary>
    /// 工作单元实现类，封装 EF DbContext 的事务管理和保存操作
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;
        private IDbContextTransaction? _transaction;

        /// <summary>
        /// 构造函数，注入 DbContext 实例
        /// </summary>
        /// <param name="context">EF 数据上下文</param>
        public UnitOfWork(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 保存所有挂起的更改到数据库
        /// </summary>
        /// <returns>受影响的记录数</returns>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 开启数据库事务（如果未开启）
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            if (_transaction == null)
            {
                _transaction = await _context.Database.BeginTransactionAsync();
            }
        }

        /// <summary>
        /// 提交当前事务并释放资源
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// 回滚当前事务并释放资源
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// 释放上下文及事务资源
        /// </summary>
        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
