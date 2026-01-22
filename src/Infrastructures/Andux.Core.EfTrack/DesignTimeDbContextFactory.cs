using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Andux.Core.EfTrack
{
    /// <summary>
    /// 设计期 DbContext 工厂基类
    /// 
    /// 主要用于 EF Core 在设计期（Design-Time）
    /// 执行迁移、更新数据库等命令时创建 DbContext 实例，
    /// 避免启动完整的应用 Host 及依赖注入管道。
    /// 
    /// 该工厂负责：
    /// 1. 从配置文件中读取数据库连接字符串；
    /// 2. 构建 DbContextOptions 并指定数据库提供程序；
    /// 3. 手动构造并注入 EntityBehaviorOptions 等运行期依赖；
    /// 4. 为 EF Core 提供一个最小、可控、稳定的 DbContext 创建方式。
    /// 
    /// 通过实现 <see cref="IDesignTimeDbContextFactory{TContext}"/>，
    /// 可确保在执行 dotnet ef migrations / update 等命令时，
    /// 不依赖 Program.cs 或 WebApplicationBuilder，
    /// 有效避免外部服务调用、网络请求或环境副作用。
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class DesignTimeDbContextFactory<TContext> : IDesignTimeDbContextFactory<TContext>
        where TContext : DbContext
    {
        private readonly string _configFile;
        private readonly string _connectionStringName;

        /// <summary>
        /// 公用 Factory 构造函数
        /// </summary>
        /// <param name="configFile">appsettings.json 文件名</param>
        /// <param name="connectionStringName">连接串名</param>
        public DesignTimeDbContextFactory(string configFile = "appsettings.json",
            string connectionStringName = "Default")
        {
            _configFile = configFile;
            _connectionStringName = connectionStringName;
        }

        public virtual TContext CreateDbContext(string[] args)
        {
            #region 读取 appsettings.json 方式

            var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile(_configFile, optional: false)
               .AddEnvironmentVariables()
               .Build();

            var connStr = configuration.GetConnectionString(_connectionStringName);
            if (string.IsNullOrWhiteSpace(connStr))
                throw new InvalidOperationException($"ConnectionString {_connectionStringName} 未配置");

            var optionsBuilder = new DbContextOptionsBuilder<TContext>();
            optionsBuilder.UseMySql(connStr, ServerVersion.AutoDetect(connStr));

            // 尝试绑定 EntityBehaviorOptions（可选）
            var behaviorOptions = Options.Create(
                configuration.GetSection(nameof(EntityBehaviorOptions))
                    .Get<EntityBehaviorOptions>() ?? new EntityBehaviorOptions());

            // 利用反射创建 DbContext
            return (TContext)Activator.CreateInstance(typeof(TContext), optionsBuilder.Options, behaviorOptions)!;

            #endregion

        }
    }
}
