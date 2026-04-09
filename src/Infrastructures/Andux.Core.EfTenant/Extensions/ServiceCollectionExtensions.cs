using Andux.Core.EfTenant.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Andux.Core.EfTenant.Extensions
{
    /// <summary>
    /// 服务集合扩展
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加多租户支持
        /// </summary>
        /// <typeparam name="TContext">DbContext 类型</typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddMultiTenantKit<TContext>(
            this IServiceCollection services,
            IConfiguration configuration)
            where TContext : DbContext
        {
            services.AddHttpContextAccessor();

            services.AddScoped<ITenantProvider, DefaultTenantProvider>();
            services.AddScoped<ITenantStore, TenantStore>();

            // 注册 DbContext 工厂
            services.AddScoped<TenantDbContextFactory<TContext>>();

            services.Configure<EntityBehaviorOptions>(configuration.GetSection("EntityBehaviorOptions"));
            services.AddScoped<AuditingInterceptor>();

            // 注册工作单元（使用泛型实现）
            services.AddScoped<ITenantUnitOfWork, TenantUnitOfWork<TContext>>();

            // 注册仓储
            services.AddScoped(typeof(ITenantRepository<>), typeof(EfTenantRepository<>));

            // 添加内存缓存
            services.AddMemoryCache();

            return services;
        }

    }
}
