// =======================================
// 作者：andy.hu
// 文件：IRepository.cs
// 描述：泛型仓储接口定义，封装常见 CRUD 操作与条件查询、分页、聚合统计等
// =======================================

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Threading.Tasks;
using Andux.Core.EfTrack.Repository.Paged;

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
        /// 根据主键获取实体（包含导航属性）
        /// </summary>
        /// <param name="id">主键值</param>
        /// <returns>实体（包含导航属性）</returns>
        Task<T?> GetByIdWithIncludesAsync(object id);

        /// <summary>
        /// 根据主键获取实体（支持指定导航属性 Include）
        /// </summary>
        /// <param name="id">主键值</param>
        /// <param name="includes">要 Include 的导航属性</param>
        /// <returns>实体（包含指定导航属性）</returns>
        Task<T?> GetByIdWithIncludesAsync(object id, params Expression<Func<T, object>>[] includes);

        /// <summary>
        /// 根据主键获取实体（支持指定导航属性 Include）
        /// </summary>
        /// <param name="id">主键值</param>
        /// <param name="includes">要 Include 的导航属性名称</param>
        /// <returns>实体（包含指定导航属性）</returns>
        Task<T?> GetByIdWithIncludesAsync(object id, params string[] includes);

        /// <summary>
        /// 分页查询数据
        /// </summary>
        /// <param name="pageParam">基础分页参数</param>
        /// <param name="predicate">筛选条件</param>
        /// <param name="orderBy">排序</param>
        /// <returns></returns>
        Task<PagedResult<T>> GetPagedAsync(BasePageParam pageParam,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);

        /// <summary>
        /// 分页查询数据（支持 Include 指定导航属性）
        /// </summary>
        /// <param name="pageParam">分页参数</param>
        /// <param name="predicate">筛选条件</param>
        /// <param name="orderBy">排序条件</param>
        /// <param name="includes">导航属性 Include 表达式</param>
        /// <returns>分页结果</returns>
        Task<PagedResult<T>> GetPagedWithIncludesAsync(
            BasePageParam pageParam,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Expression<Func<T, object>>[] includes);

        /// <summary>
        /// 分页查询数据（支持 Include 指定导航属性）
        /// </summary>
        /// <param name="pageParam">分页参数</param>
        /// <param name="includes">导航属性 Include 表达式</param>
        /// <returns>分页结果</returns>
        Task<PagedResult<T>> GetPagedWithIncludesAsync(
            BasePageParam pageParam,
            params Expression<Func<T, object>>[] includes);

        /// <summary>
        /// 分页查询数据（支持字符串指定导航属性 Include）
        /// </summary>
        /// <param name="pageParam">分页参数</param>
        /// <param name="predicate">筛选条件</param>
        /// <param name="orderBy">排序条件</param>
        /// <param name="includes">要 Include 的导航属性名称</param>
        /// <returns>分页结果</returns>
        Task<PagedResult<T>> GetPagedWithIncludesAsync(
            BasePageParam pageParam,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params string[] includes);

        /// <summary>
        /// 分页查询数据（支持 Include 指定导航属性）
        /// </summary>
        /// <param name="pageParam">分页参数</param>
        /// <param name="includes">要 Include 的导航属性名称</param>
        /// <returns></returns>
        Task<PagedResult<T>> GetPagedWithIncludesAsync(
            BasePageParam pageParam,
            params string[] includes);

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
        /// 批量更新数据（基于条件 + 更新表达式）
        /// </summary>
        /// <param name="predicate">筛选条件</param>
        /// <param name="updateExpression">更新内容表达式</param>
        Task<int> UpdateRangeAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, T>> updateExpression);

        /// <summary>
        /// 删除实体
        /// </summary>
        void Remove(T entity);

        /// <summary>
        /// 批量删除
        /// </summary>
        void RemoveRange(IEnumerable<T> entities);

        /// <summary>
        /// 按条件软删除（逻辑删除）
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<int> RemoveAsync(Expression<Func<T, bool>> predicate);

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

        /// <summary>
        /// 查询第一个符合条件的实体（如无结果返回 null）。
        /// 支持动态包含导航属性。
        /// </summary>
        /// <param name="predicate">查询条件表达式。</param>
        /// <param name="includes">可选导航属性名称（支持多个）。</param>
        /// <returns>第一个符合条件的实体或 null。</returns>
        Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            params string[] includes);

        /// <summary>
        /// 查询第一个符合条件的实体（如无结果返回 null）。
        /// 支持动态包含导航属性（通过表达式）。
        /// </summary>
        /// <param name="predicate">查询条件表达式。</param>
        /// <param name="includes">要包含的导航属性表达式（支持多个）。</param>
        /// <returns>第一个符合条件的实体或 null。</returns>
        Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes);

        /// <summary>
        /// 根据条件对指定字段求和。
        /// </summary>
        /// <param name="predicate">筛选条件。</param>
        /// <param name="selector">求和字段选择器。</param>
        /// <returns>返回满足条件的字段求和值。</returns>
        Task<decimal> SumAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, decimal>> selector);

        /// <summary>
        /// 根据条件对指定字段求平均值。
        /// </summary>
        /// <param name="predicate">筛选条件。</param>
        /// <param name="selector">求平均值字段选择器。</param>
        /// <returns>返回满足条件的字段平均值。</returns>
        Task<decimal> AverageAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, decimal>> selector);

        /// <summary>
        /// 根据条件对指定字段求最大值。
        /// </summary>
        /// <typeparam name="TProperty">字段类型。</typeparam>
        /// <param name="predicate">筛选条件。</param>
        /// <param name="selector">求最大值字段选择器。</param>
        /// <returns>返回满足条件的字段最大值。</returns>
        Task<TProperty> MaxAsync<TProperty>(Expression<Func<T, bool>> predicate,
            Expression<Func<T, TProperty>> selector);

        /// <summary>
        /// 根据条件对指定字段求最小值。
        /// </summary>
        /// <typeparam name="TProperty">字段类型。</typeparam>
        /// <param name="predicate">筛选条件。</param>
        /// <param name="selector">求最小值字段选择器。</param>
        /// <returns>返回满足条件的字段最小值。</returns>
        Task<TProperty> MinAsync<TProperty>(Expression<Func<T, bool>> predicate,
            Expression<Func<T, TProperty>> selector);

        /// <summary>
        /// 根据条件查询指定字段的去重列表。
        /// </summary>
        /// <typeparam name="TProperty">去重字段的类型。</typeparam>
        /// <param name="predicate">筛选条件。</param>
        /// <param name="selector">字段选择器。</param>
        /// <returns>去重后的字段值集合。</returns>
        Task<List<TProperty>> DistinctAsync<TProperty>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TProperty>> selector);

        /// <summary>
        /// 根据条件分组并投影结果。
        /// </summary>
        /// <typeparam name="TKey">分组字段类型。</typeparam>
        /// <typeparam name="TResult">返回结果类型。</typeparam>
        /// <param name="predicate">筛选条件。</param>
        /// <param name="groupBySelector">分组字段选择器。</param>
        /// <param name="resultSelector">分组投影选择器。</param>
        /// <returns>分组统计后的结果集合。</returns>
        Task<List<TResult>> GroupByAsync<TKey, TResult>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TKey>> groupBySelector,
            Expression<Func<IGrouping<TKey, T>, TResult>> resultSelector);
    }
}
