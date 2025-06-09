// =======================================
// 作者：andy.hu
// 文件：IRepository.cs
// 描述：泛型仓储接口定义，封装常见 CRUD 操作与条件查询、分页等
// =======================================

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Threading.Tasks;

namespace Andux.Core.EfTrack
{
    /// <summary>
    /// 泛型仓储接口，定义常见数据访问操作
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// 根据主键获取实体
        /// </summary>
        Task<T?> GetByIdAsync(object id);

        /// <summary>
        /// 获取所有实体
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// 条件查询实体集
        /// </summary>
        IQueryable<T> Query(Expression<Func<T, bool>>? predicate = null);

        /// <summary>
        /// 添加实体
        /// </summary>
        Task AddAsync(T entity);

        /// <summary>
        /// 添加多个实体
        /// </summary>
        Task AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// 更新实体
        /// </summary>
        void Update(T entity);

        /// <summary>
        /// 删除实体
        /// </summary>
        void Remove(T entity);

        /// <summary>
        /// 批量删除
        /// </summary>
        void RemoveRange(IEnumerable<T> entities);

        /// <summary>
        /// 根据条件判断是否存在
        /// </summary>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 计数（可选条件）
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        /// <summary>
        /// 查询单个实体（可为空）
        /// </summary>
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    }
}
