// =======================================
// 作者：andy.hu
// 文件：Repository.cs
// 描述：泛型仓储接口实现，封装常见 CRUD 操作与条件查询、分页、聚合统计等
// =======================================

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Andux.Core.EfTrack.Entities;
using Andux.Core.EfTrack.Repository.Paged;

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
        private readonly EntityBehaviorOptions _options;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context"></param>
        /// <param name="options"></param>
        /// <param name="accessor"></param>
        public Repository(DbContext context, 
            IOptions<EntityBehaviorOptions> options, 
            IHttpContextAccessor accessor)
        {
            _context = context;
            _dbSet = context.Set<T>();

            _options = options.Value;
            _httpContextAccessor = accessor;
        }

        /// <summary>
        /// 根据主键获取实体
        /// </summary>
        public async Task<T?> GetByIdAsync(object id)
        {
            var keyProperty = _context.Model.FindEntityType(typeof(T))?.FindPrimaryKey()?.Properties.FirstOrDefault();
            if (keyProperty == null)
                throw new InvalidOperationException("找不到主键属性");

            var parameter = Expression.Parameter(typeof(T), "e");
            var property = Expression.Property(parameter, keyProperty.Name);
            var constant = Expression.Constant(id);
            var equal = Expression.Equal(property, Expression.Convert(constant, property.Type));
            var lambda = Expression.Lambda<Func<T, bool>>(equal, parameter);

            return await ApplyProjectFilter(_dbSet).FirstOrDefaultAsync(lambda);
        }

        /// <summary>
        /// 根据主键获取实体（包含导航属性）
        /// </summary>
        /// <param name="id">主键值</param>
        /// <returns>实体（包含导航属性）</returns>
        public async Task<T?> GetByIdWithIncludesAsync(object id)
        {
            var entityType = _context.Model.FindEntityType(typeof(T));
            var keyProperty = entityType?.FindPrimaryKey()?.Properties.FirstOrDefault();

            if (keyProperty == null)
                throw new InvalidOperationException("找不到主键属性");

            // 构造表达式 e => e.Id == id
            var parameter = Expression.Parameter(typeof(T), "e");
            var property = Expression.Property(parameter, keyProperty.Name);
            var constant = Expression.Constant(id);
            var equal = Expression.Equal(property, Expression.Convert(constant, property.Type));
            var lambda = Expression.Lambda<Func<T, bool>>(equal, parameter);

            // 构造 query 并自动 Include 所有导航属性
            IQueryable<T> query = ApplyProjectFilter(_dbSet);
            foreach (var nav in entityType.GetNavigations())
            {
                query = query.Include(nav.Name);
            }

            return await query.FirstOrDefaultAsync(lambda);
        }

        /// <summary>
        /// 根据主键获取实体（支持指定导航属性 Include）
        /// </summary>
        /// <param name="id">主键值</param>
        /// <param name="includes">要 Include 的导航属性</param>
        /// <returns>实体（包含指定导航属性）</returns>
        public async Task<T?> GetByIdWithIncludesAsync(object id, params Expression<Func<T, object>>[] includes)
        {
            var entityType = _context.Model.FindEntityType(typeof(T));
            var keyProperty = entityType?.FindPrimaryKey()?.Properties.FirstOrDefault();

            if (keyProperty == null)
                throw new InvalidOperationException("找不到主键属性");

            // 构造主键表达式
            var parameter = Expression.Parameter(typeof(T), "e");
            var property = Expression.Property(parameter, keyProperty.Name);
            var constant = Expression.Constant(id);
            var equal = Expression.Equal(property, Expression.Convert(constant, property.Type));
            var lambda = Expression.Lambda<Func<T, bool>>(equal, parameter);

            // 构造 query
            IQueryable<T> query = ApplyProjectFilter(_dbSet);

            // 动态 Include 指定属性
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(lambda);
        }

        /// <summary>
        /// 根据主键获取实体（支持指定导航属性 Include）
        /// </summary>
        /// <param name="id">主键值</param>
        /// <param name="includes">要 Include 的导航属性名称</param>
        /// <returns>实体（包含指定导航属性）</returns>
        public async Task<T?> GetByIdWithIncludesAsync(object id, params string[] includes)
        {
            var entityType = _context.Model.FindEntityType(typeof(T));
            var keyProperty = entityType?.FindPrimaryKey()?.Properties.FirstOrDefault();

            if (keyProperty == null)
                throw new InvalidOperationException("找不到主键属性");

            var parameter = Expression.Parameter(typeof(T), "e");
            var property = Expression.Property(parameter, keyProperty.Name);
            var constant = Expression.Constant(id);
            var equal = Expression.Equal(property, Expression.Convert(constant, property.Type));
            var lambda = Expression.Lambda<Func<T, bool>>(equal, parameter);

            IQueryable<T> query = ApplyProjectFilter(_dbSet);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(lambda);
        }

        /// <summary>
        /// 分页查询数据
        /// </summary>
        /// <param name="pageParam"></param>
        /// <param name="predicate"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public async Task<PagedResult<T>> GetPagedAsync(
            BasePageParam pageParam,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            var query = ApplyProjectFilter(_dbSet);

            if (predicate != null)
                query = query.Where(predicate);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageParam.Limit);

            if (orderBy != null)
                query = orderBy(query);

            var items = await query.Skip((pageParam.Page - 1) * pageParam.Limit).Take(pageParam.Limit).ToListAsync();

            return new PagedResult<T>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items
            };
        }

        /// <summary>
        /// 分页查询数据（支持 Include 指定导航属性）
        /// </summary>
        /// <param name="pageParam">分页参数</param>
        /// <param name="predicate">筛选条件</param>
        /// <param name="orderBy">排序条件</param>
        /// <param name="includes">导航属性 Include 表达式</param>
        /// <returns>分页结果</returns>
        public async Task<PagedResult<T>> GetPagedWithIncludesAsync(
            BasePageParam pageParam,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Expression<Func<T, object>>[] includes)
        {
            var query = ApplyProjectFilter(_dbSet);

            if (predicate != null)
                query = query.Where(predicate);

            // 动态 Include
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageParam.Limit);

            if (orderBy != null)
                query = orderBy(query);

            var items = await query
                .Skip((pageParam.Page - 1) * pageParam.Limit)
                .Take(pageParam.Limit)
                .ToListAsync();

            return new PagedResult<T>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items
            };
        }

        /// <summary>
        /// 分页查询数据（支持 Include 指定导航属性）
        /// </summary>
        /// <param name="pageParam">分页参数</param>
        /// <param name="includes">导航属性 Include 表达式</param>
        /// <returns>分页结果</returns>
        public Task<PagedResult<T>> GetPagedWithIncludesAsync(
            BasePageParam pageParam,
            params Expression<Func<T, object>>[] includes)
        {
            return GetPagedWithIncludesAsync(pageParam, null, null, includes);
        }

        /// <summary>
        /// 分页查询数据（支持字符串指定导航属性 Include）
        /// </summary>
        /// <param name="pageParam">分页参数</param>
        /// <param name="predicate">筛选条件</param>
        /// <param name="orderBy">排序条件</param>
        /// <param name="includes">要 Include 的导航属性名称</param>
        /// <returns>分页结果</returns>
        public async Task<PagedResult<T>> GetPagedWithIncludesAsync(
            BasePageParam pageParam,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params string[] includes)
        {
            var query = ApplyProjectFilter(_dbSet);

            if (predicate != null)
                query = query.Where(predicate);

            // 动态 Include
            if (includes != null)
            {
                foreach (var include in includes.Where(i => !string.IsNullOrWhiteSpace(i)))
                {
                    query = query.Include(include);
                }
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageParam.Limit);

            if (orderBy != null)
                query = orderBy(query);

            var items = await query
                .Skip((pageParam.Page - 1) * pageParam.Limit)
                .Take(pageParam.Limit)
                .ToListAsync();

            return new PagedResult<T>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items
            };
        }

        /// <summary>
        /// 分页查询数据（支持 Include 指定导航属性）
        /// </summary>
        /// <param name="pageParam">分页参数</param>
        /// <param name="includes">要 Include 的导航属性名称</param>
        /// <returns></returns>
        public Task<PagedResult<T>> GetPagedWithIncludesAsync(
            BasePageParam pageParam,
            params string[] includes)
        {
            return GetPagedWithIncludesAsync(pageParam, null, null, includes);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await ApplyProjectFilter(_dbSet).ToListAsync();
        }

        /// <summary>
        /// 条件查询
        /// </summary>
        public IQueryable<T> Query(Expression<Func<T, bool>>? predicate = null)
        {
            var query = ApplyProjectFilter(_dbSet);
            return predicate != null ? query.Where(predicate) : query;
        }

        /// <summary>
        /// 添加单个实体
        /// </summary>
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        /// <summary>
        /// 批量添加实体
        /// </summary>
        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        /// <summary>
        /// 更新实体
        /// </summary>
        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        /// <summary>
        /// 批量更新数据（基于条件 + 更新表达式）
        /// </summary>
        /// <param name="predicate">筛选条件</param>
        /// <param name="updateExpression">更新内容表达式</param>
        public async Task<int> UpdateRangeAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, T>> updateExpression)
        {
            var entities = await _dbSet.Where(predicate).ToListAsync();

            var compiledUpdate = updateExpression.Compile();
            foreach (var entity in entities)
            {
                var updatedEntity = compiledUpdate(entity);
                _context.Entry(entity).CurrentValues.SetValues(updatedEntity);
            }

            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        /// <summary>
        /// 批量删除实体
        /// </summary>
        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        /// <summary>
        /// 按条件软删除（逻辑删除）
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<int> RemoveAsync(Expression<Func<T, bool>> predicate)
        {
            if (!typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
                throw new InvalidOperationException($"{typeof(T).Name} 未实现 ISoftDelete，不支持调用此方法删除");

            var entities = await _dbSet.Where(predicate).Cast<ISoftDelete>().ToListAsync();
            foreach (var entity in entities)
            {
                entity.IsDeleted = true;
            }

            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 判断是否存在符合条件的实体
        /// </summary>
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await ApplyProjectFilter(_dbSet).AnyAsync(predicate);
        }

        /// <summary>
        /// 获取符合条件的实体数量
        /// </summary>
        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            var query = ApplyProjectFilter(_dbSet);
            return predicate == null ? await query.CountAsync() : await query.CountAsync(predicate);
        }

        /// <summary>
        /// 查询第一个符合条件的实体（如无结果返回 null）
        /// </summary>
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await ApplyProjectFilter(_dbSet).FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// 查询第一个符合条件的实体（如无结果返回 null）。
        /// 支持动态包含导航属性。
        /// </summary>
        /// <param name="predicate">查询条件表达式。</param>
        /// <param name="includes">可选导航属性名称（支持多个）。</param>
        /// <returns>第一个符合条件的实体或 null。</returns>
        public async Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            params string[] includes)
        {
            IQueryable<T> query = ApplyProjectFilter(_dbSet);
            if (includes is { Length: > 0 })
            {
                foreach (var include in includes.Where(i => !string.IsNullOrWhiteSpace(i)))
                {
                    query = query.Include(include);
                }
            }

            return await query.FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// 查询第一个符合条件的实体（如无结果返回 null）。
        /// 支持动态包含导航属性（通过表达式）。
        /// </summary>
        /// <param name="predicate">查询条件表达式。</param>
        /// <param name="includes">要包含的导航属性表达式（支持多个）。</param>
        /// <returns>第一个符合条件的实体或 null。</returns>
        public async Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = ApplyProjectFilter(_dbSet);

            if (includes is { Length: > 0 })
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// 根据条件对指定字段求和。
        /// </summary>
        /// <param name="predicate">筛选条件。</param>
        /// <param name="selector">求和字段选择器。</param>
        /// <returns>返回满足条件的字段求和值。</returns>
        public async Task<decimal> SumAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, decimal>> selector)
        {
            return await _dbSet.Where(predicate).SumAsync(selector);
        }

        /// <summary>
        /// 根据条件对指定字段求平均值。
        /// </summary>
        /// <param name="predicate">筛选条件。</param>
        /// <param name="selector">求平均值字段选择器。</param>
        /// <returns>返回满足条件的字段平均值。</returns>
        public async Task<decimal> AverageAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, decimal>> selector)
        {
            return await _dbSet.Where(predicate).AverageAsync(selector);
        }

        /// <summary>
        /// 根据条件对指定字段求最大值。
        /// </summary>
        /// <typeparam name="TProperty">字段类型。</typeparam>
        /// <param name="predicate">筛选条件。</param>
        /// <param name="selector">求最大值字段选择器。</param>
        /// <returns>返回满足条件的字段最大值。</returns>
        public async Task<TProperty> MaxAsync<TProperty>(Expression<Func<T, bool>> predicate, Expression<Func<T, TProperty>> selector)
        {
            return await _dbSet.Where(predicate).MaxAsync(selector);
        }

        /// <summary>
        /// 根据条件对指定字段求最小值。
        /// </summary>
        /// <typeparam name="TProperty">字段类型。</typeparam>
        /// <param name="predicate">筛选条件。</param>
        /// <param name="selector">求最小值字段选择器。</param>
        /// <returns>返回满足条件的字段最小值。</returns>
        public async Task<TProperty> MinAsync<TProperty>(Expression<Func<T, bool>> predicate, Expression<Func<T, TProperty>> selector)
        {
            return await _dbSet.Where(predicate).MinAsync(selector);
        }

        /// <summary>
        /// 根据条件查询指定字段的去重列表。
        /// </summary>
        /// <typeparam name="TProperty">去重字段的类型。</typeparam>
        /// <param name="predicate">筛选条件。</param>
        /// <param name="selector">字段选择器。</param>
        /// <returns>去重后的字段值集合。</returns>
        public async Task<List<TProperty>> DistinctAsync<TProperty>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TProperty>> selector)
        {
            return await _dbSet
                .Where(predicate)
                .Select(selector)
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// 根据条件分组并投影结果。
        /// </summary>
        /// <typeparam name="TKey">分组字段类型。</typeparam>
        /// <typeparam name="TResult">返回结果类型。</typeparam>
        /// <param name="predicate">筛选条件。</param>
        /// <param name="groupBySelector">分组字段选择器。</param>
        /// <param name="resultSelector">分组投影选择器。</param>
        /// <returns>分组统计后的结果集合。</returns>
        public async Task<List<TResult>> GroupByAsync<TKey, TResult>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TKey>> groupBySelector,
            Expression<Func<IGrouping<TKey, T>, TResult>> resultSelector)
        {
            return await _dbSet
                .Where(predicate)
                .GroupBy(groupBySelector)
                .Select(resultSelector)
                .ToListAsync();
        }

        /// <summary>
        /// 自动添加项目ID过滤
        /// </summary>
        private IQueryable<T> ApplyProjectFilter(IQueryable<T> query)
        {
            if (typeof(IProject).IsAssignableFrom(typeof(T)) && _options.EnableProject)
            {
                var currentProjectStr = _httpContextAccessor.HttpContext?.User.Claims
                    .FirstOrDefault(c => c.Type == _options.ProjectClaimsType)?.Value;

                if (!long.TryParse(currentProjectStr, out var projectIdValue))
                    return query; // 无效 ProjectId，不过滤

                var projectId = (long?)projectIdValue;

                var parameter = Expression.Parameter(typeof(T), "e");
                var property = Expression.Property(parameter, nameof(IProject.ProjectId));
                var constant = Expression.Constant(projectId, typeof(long?));
                var equal = Expression.Equal(property, constant);
                var lambda = Expression.Lambda<Func<T, bool>>(equal, parameter);

                return query.Where(lambda);
            }

            return query;
        }

    }
}
