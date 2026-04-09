// =======================================
// 作者：andy.hu
// 文件：UnitOfWork.cs
// 描述：工作单元实现类，支持租户动态切换（支持 DbContext 池化和并发）
// =======================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Andux.Core.EfTenant
{
    /// <summary>
    /// 工作单元实现类，支持租户动态切换和高并发
    /// 支持 DbContext 池化和并发
    /// </summary>
    /// <typeparam name="TContext">DbContext 类型</typeparam>
    public class TenantUnitOfWork<TContext> : ITenantUnitOfWork where TContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITenantProvider _tenantProvider;
        private readonly ConcurrentDictionary<long, TContext> _contextCache = new();
        private TContext? _currentContext;
        private long? _currentTenantId;
        private IDbContextTransaction? _transaction;
        private readonly SemaphoreSlim _contextLock = new SemaphoreSlim(1, 1);
        private bool _disposed;

        public TenantUnitOfWork(IServiceProvider serviceProvider, ITenantProvider tenantProvider)
        {
            _serviceProvider = serviceProvider;
            _tenantProvider = tenantProvider;
        }

        public DbContext GetDbContext()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TenantUnitOfWork<TContext>));

            var currentTenantId = _tenantProvider.TryGetTenantId();

            if (!currentTenantId.HasValue)
                throw new InvalidOperationException("未设置租户ID");

            // 如果租户ID变化，切换上下文
            if (_currentTenantId != currentTenantId)
            {
                _contextLock.Wait();
                try
                {
                    if (_currentTenantId != currentTenantId)
                    {
                        // 提交当前事务（如果有）
                        if (_transaction != null)
                        {
                            _transaction.Commit();
                            _transaction.Dispose();
                            _transaction = null;
                        }

                        _currentContext = GetOrCreateContext(currentTenantId.Value);
                        _currentTenantId = currentTenantId;
                    }
                }
                finally
                {
                    _contextLock.Release();
                }
            }

            return _currentContext!;
        }

        private TContext GetOrCreateContext(long tenantId)
        {
            // 先从缓存获取
            if (_contextCache.TryGetValue(tenantId, out var context))
                return context;

            // 创建新的 DbContext
            var factory = _serviceProvider.GetRequiredService<TenantDbContextFactory<TContext>>();
            var newContext = factory.CreateForTenant(tenantId);

            // 尝试添加到缓存（如果其他线程已经添加，使用已存在的）
            return _contextCache.GetOrAdd(tenantId, newContext);
        }

        public async Task<int> SaveChangesAsync()
        {
            if (_currentContext == null)
                return 0;

            return await _currentContext.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction == null && _currentContext != null)
            {
                _transaction = await _currentContext.Database.BeginTransactionAsync();
            }
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void RefreshContext()
        {
            _contextLock.Wait();
            try
            {
                _currentContext = null;
                _currentTenantId = null;

                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                }
            }
            finally
            {
                _contextLock.Release();
            }
        }

        /// <summary>
        /// 清理指定租户的缓存
        /// </summary>
        public void ClearCache(long tenantId)
        {
            if (_contextCache.TryRemove(tenantId, out var context))
            {
                context.Dispose();
            }
        }

        /// <summary>
        /// 清理所有缓存
        /// </summary>
        public void ClearAllCache()
        {
            foreach (var context in _contextCache.Values)
            {
                context.Dispose();
            }
            _contextCache.Clear();
        }

        public void Dispose()
        {
            if (_disposed) return;

            _contextLock.Wait();
            try
            {
                _transaction?.Dispose();
                foreach (var context in _contextCache.Values)
                {
                    context.Dispose();
                }
                _contextCache.Clear();
                _disposed = true;
            }
            finally
            {
                _contextLock.Release();
                _contextLock.Dispose();
            }
        }
    }
}
