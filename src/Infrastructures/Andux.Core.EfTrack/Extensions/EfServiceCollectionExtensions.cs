// =======================================
// 作者：andy.hu
// 文件：EfServiceCollectionExtensions.cs
// 描述：ef 服务扩展
// =======================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Andux.Core.EfTrack
{
    /// <summary>
    /// ef 服务扩展
    /// </summary>
    public static class EfServiceCollectionExtensions
    {
        /// <summary>
        /// 注册 EF 仓储、工作单元、审计拦截器、DbContext（MySQL）
        /// </summary>
        public static IServiceCollection AddEfOrmFramework<TContext>(
            this IServiceCollection services,
            IConfiguration configuration,
            Version? mySqlVersion = null
        ) where TContext : DbContext
        {
            services.Configure<EntityBehaviorOptions>(configuration.GetSection("EntityBehaviorOptions"));
            services.AddScoped<AuditingInterceptor>();

            // 注册仓储与工作单元
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<DbContext, TContext>();

            // 注册 DbContext 与拦截器（自动读取连接串）
            services.AddDbContext<TContext>((provider, options) =>
            {
                var interceptor = provider.GetRequiredService<AuditingInterceptor>();
                options.UseMySql(configuration.GetConnectionString("Default"),
                    // 替换为实际 MySQL 版本
                    new MySqlServerVersion(mySqlVersion) 
                ).AddInterceptors(interceptor);
            });

            return services;
        }

    }
}
