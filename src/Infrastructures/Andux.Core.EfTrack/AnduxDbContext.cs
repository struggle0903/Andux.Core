// =======================================
// 作者：andy.hu
// 文件：AnduxDbContext.cs
// 描述：统一封装软删除逻辑的基础 DbContext
// =======================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Andux.Core.EfTrack
{
    /// <summary>
    /// 提供软删除过滤的基础 DbContext，可供具体上下文继承
    /// </summary>
    public abstract class AnduxDbContext : DbContext
    {
        private readonly EntityBehaviorOptions _options;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options"></param>
        /// <param name="behaviorOptions"></param>
        protected AnduxDbContext(DbContextOptions options, 
            IOptions<EntityBehaviorOptions> behaviorOptions)
            : base(options)
        {
            _options = behaviorOptions.Value;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 加软删除查询过滤器
            ApplySoftDeleteQueryFilter(modelBuilder);
        }

        /// <summary>
        /// 添加软删除查询过滤器
        /// </summary>
        private void ApplySoftDeleteQueryFilter(ModelBuilder modelBuilder)
        {
            if (!_options.EnableSoftDelete) return;

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var prop = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                    var condition = Expression.Equal(prop, Expression.Constant(false));
                    var lambda = Expression.Lambda(condition, parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            HandleSoftDelete();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            HandleSoftDelete();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 处理软删除逻辑：将删除操作转换为修改状态并设置 IsDeleted 标志
        /// </summary>
        private void HandleSoftDelete()
        {
            if (!_options.EnableSoftDelete) return;

            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted))
            {
                if (entry.Entity is ISoftDelete softDelete)
                {
                    entry.State = EntityState.Modified;
                    softDelete.IsDeleted = true;
                }
            }
        }

        /// <summary>
        /// 注册实体
        /// </summary>
        /// <param name="modelBuilder"></param>
        /// <param name="assemblyMarkerTypes"></param>
        protected void RegisterEntities(ModelBuilder modelBuilder, params Type[] assemblyMarkerTypes)
        {
            var allTypes = assemblyMarkerTypes
                .SelectMany(t => t.Assembly.GetExportedTypes())
                .Where(t =>
                    t is { IsClass: true, IsAbstract: false } &&
                    t.GetInterfaces().Any(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>)))
                .Distinct();

            foreach (var type in allTypes)
            {
                modelBuilder.Entity(type);
            }
        }

    }
}
