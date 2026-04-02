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
        /// <typeparam name="TContext"></typeparam>
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

            services.AddScoped<TenantDbContextFactory<TContext>>();

            services.Configure<EntityBehaviorOptions>(configuration.GetSection("EntityBehaviorOptions"));
            services.AddScoped<AuditingInterceptor>();

            // DbContext 动态创建
            services.AddScoped<DbContext>(sp =>
            {
                var factory = sp.GetRequiredService<TenantDbContextFactory<TContext>>();
                return factory.Create();
            });

            // 添加内存缓存
            services.AddMemoryCache();

            services.AddScoped(typeof(ITenantRepository<>), typeof(EfTenantRepository<>));
            services.AddScoped<ITenantUnitOfWork, TenantUnitOfWork>();

            return services;
        }
    }
}
