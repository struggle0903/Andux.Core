using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Andux.Core.EfTenant
{
    /// <summary>
    /// 租户级 DbContext 动态工厂
    /// </summary>
    /// <remarks>
    /// 根据当前租户标识动态获取对应的数据库连接字符串，创建隔离的 DbContext 实例。
    /// 支持：
    /// <list type="bullet">
    /// <item><description>多租户数据库隔离：每个租户使用独立的数据库连接</description></item>
    /// <item><description>审计拦截器自动注入：自动记录数据变更日志</description></item>
    /// <item><description>依赖注入集成：通过 IServiceProvider 创建实例</description></item>
    /// </list>
    /// </remarks>
    /// <typeparam name="TContext">DbContext 类型，必须继承自 <see cref="DbContext"/></typeparam>
    public class TenantDbContextFactory<TContext>
        where TContext : DbContext
    {
        private readonly IServiceProvider _sp;
        private readonly ITenantProvider _tenantProvider;
        private readonly ITenantStore _tenantStore;
        private readonly IMemoryCache _cache;

        /// <summary>
        /// 初始化租户 DbContext 工厂实例
        /// </summary>
        /// <param name="sp">服务提供者，用于创建 DbContext 实例和解析依赖服务</param>
        /// <param name="tenantProvider">租户提供者，用于获取当前请求的租户标识</param>
        /// <param name="tenantStore">租户存储，用于获取租户对应的数据库连接字符串</param>
        /// <param name="cache"></param>
        public TenantDbContextFactory(IServiceProvider sp,
            ITenantProvider tenantProvider,
            ITenantStore tenantStore,
            IMemoryCache cache)
        {
            _sp = sp;
            _tenantProvider = tenantProvider;
            _tenantStore = tenantStore;
            _cache = cache;
        }

        /// <summary>
        /// 创建当前租户对应的 DbContext 实例
        /// </summary>
        /// <returns>
        /// 已配置好数据库连接和审计拦截器的 <typeparamref name="TContext"/> 实例
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// 当租户标识无效或无法获取连接字符串时抛出
        /// </exception>
        public TContext Create()
        {
            var tenantId = _tenantProvider.TenantId;
            var options = _cache.GetOrCreate($"db_options_{tenantId}", entry =>
            {
                // 缓存过期时间为10分钟
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                var conn = _tenantStore.GetConnectionString(tenantId);
                var interceptor = _sp.GetRequiredService<AuditingInterceptor>();

                return new DbContextOptionsBuilder<TContext>()
                    .UseMySql(conn, new MySqlServerVersion(new Version(8, 0, 36)))
                    .AddInterceptors(interceptor)
                    .Options;
            });

            return ActivatorUtilities.CreateInstance<TContext>(_sp, options);
        }

    }
}
