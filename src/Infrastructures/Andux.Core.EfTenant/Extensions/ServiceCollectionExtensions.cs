using Andux.Core.EfTenant.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Andux.Core.EfTenant.Extensions
{
    public static class ServiceCollectionExtensions
    {
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

            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
