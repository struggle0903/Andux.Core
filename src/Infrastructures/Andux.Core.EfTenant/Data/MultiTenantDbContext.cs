using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using System.Reflection;

namespace Andux.Core.EfTenant
{
    /// <summary>
    /// 多租户 DbContext（分库模式）
    /// </summary>
    public class MultiTenantDbContext : DbContext
    {
        private readonly ITenantProvider _tenantProvider;
        private readonly bool _isDesignTime;
        private readonly EntityBehaviorOptions _options;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options">DbContext 配置选项</param>
        /// <param name="tenantProvider">租户提供器（设计时可传入 null）</param>
        protected MultiTenantDbContext(
            DbContextOptions options,
            ITenantProvider tenantProvider,
            IOptions<EntityBehaviorOptions> behaviorOptions)
            : base(options)
        {
            _tenantProvider = tenantProvider;
            _options = behaviorOptions?.Value ?? new EntityBehaviorOptions();
        }

        /// <summary>
        /// 是否自动扫描并注册实体（默认 true）
        /// 自动搜索并注入继承了 IEntity 泛型接口的所有实体
        /// </summary>
        protected virtual bool AutoRegisterEntities => true;

        /// <summary>
        /// 自动扫描的程序集（子类可 override 精确控制）
        /// </summary>
        protected virtual IEnumerable<Assembly> EntityAssemblies => [GetType().Assembly];

        /// <summary>
        /// 配置模型
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            if (AutoRegisterEntities)
            {
                RegisterEntities(modelBuilder, EntityAssemblies);
            }

            // 加软删除查询过滤器
            ApplySoftDeleteQueryFilter(modelBuilder);
        }

        /// <summary>
        /// 自动注入继承了ITenantEntity泛型接口的所有实体
        /// </summary>
        /// <param name="modelBuilder"></param>
        /// <param name="assemblies"></param>
        protected void RegisterEntities(ModelBuilder modelBuilder, IEnumerable<Assembly> assemblies)
        {
            var entityTypes = assemblies
                .SelectMany(a => a.GetExportedTypes())
                .Where(t =>
                    t is { IsClass: true, IsAbstract: false } &&
                    t.GetInterfaces().Any(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(ITenantEntity<>)))
                .Distinct();

            foreach (var type in entityTypes)
            {
                modelBuilder.Entity(type);
            }
        }

        /// <summary>
        /// 添加软删除查询过滤器
        /// </summary>
        private void ApplySoftDeleteQueryFilter(ModelBuilder modelBuilder)
        {
            if (!_options.EnableSoftDelete) return;

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ITenantSoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var prop = Expression.Property(parameter, nameof(ITenantSoftDelete.IsDeleted));
                    var condition = Expression.Equal(prop, Expression.Constant(false));
                    var lambda = Expression.Lambda(condition, parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

    }
}
