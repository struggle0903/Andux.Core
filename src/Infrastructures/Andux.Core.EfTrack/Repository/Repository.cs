// =======================================
// 作者：andy.hu
// 文件：Repository.cs
// 描述：泛型仓储接口实现，封装常见 CRUD 操作与条件查询、分页等
// =======================================

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Andux.Core.EfTrack
{
    /// <summary>
    /// 泛型仓储实现类
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context"></param>
        public Repository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <summary>
        /// 根据主键获取实体
        /// </summary>
        public async Task<T?> GetByIdAsync(object id) => await _dbSet.FindAsync(id);

        /// <summary>
        /// 获取所有实体
        /// </summary>
        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        /// <summary>
        /// 条件查询
        /// </summary>
        public IQueryable<T> Query(Expression<Func<T, bool>>? predicate = null)
        {
            return predicate == null ? _dbSet : _dbSet.Where(predicate);
        }

        /// <summary>
        /// 添加单个实体
        /// </summary>
        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        /// <summary>
        /// 批量添加实体
        /// </summary>
        public async Task AddRangeAsync(IEnumerable<T> entities) => await _dbSet.AddRangeAsync(entities);

        /// <summary>
        /// 更新实体
        /// </summary>
        public void Update(T entity) => _dbSet.Update(entity);

        /// <summary>
        /// 删除实体
        /// </summary>
        public void Remove(T entity) => _dbSet.Remove(entity);

        /// <summary>
        /// 批量删除实体
        /// </summary>
        public void RemoveRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);

        /// <summary>
        /// 判断是否存在符合条件的实体
        /// </summary>
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.AnyAsync(predicate);

        /// <summary>
        /// 获取符合条件的实体数量
        /// </summary>
        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
            => predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);

        /// <summary>
        /// 查询第一个符合条件的实体（如无结果返回 null）
        /// </summary>
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.FirstOrDefaultAsync(predicate);
    }
}
