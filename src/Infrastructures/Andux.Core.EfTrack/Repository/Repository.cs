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
