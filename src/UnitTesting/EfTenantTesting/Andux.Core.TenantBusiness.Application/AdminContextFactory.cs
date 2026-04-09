using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Andux.Core.EfTenant;
using MySqlConnector;
using Microsoft.Extensions.Options;

namespace Andux.Core.TenantTesting.Application
{
    /// <summary>
    /// AdminContext 设计时工厂，用于 EF Core 迁移和工具支持
    /// </summary>
    public class AdminContextFactory : IDesignTimeDbContextFactory<AdminContext>
    {
        // 手动更新时要执行迁移记录的租户ID
        private const long DesignTimeTenantId = 1843473246985555555;

        /// <summary>
        /// 创建 AdminContext 实例，供 EF Core 设计时工具使用
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public AdminContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var baseConn = config.GetConnectionString("Default");
            var builder = new MySqlConnectionStringBuilder(baseConn);
            builder.Database = $"{builder.Database}_{DesignTimeTenantId}";

            var conn = builder.ConnectionString;
            var options = new DbContextOptionsBuilder<AdminContext>()
                .UseMySql(conn, ServerVersion.AutoDetect(conn))
                .Options;

            var behaviorOptions = new EntityBehaviorOptions { EnableSoftDelete = false };
            return new AdminContext(options, new DesignTimeTenantProvider(DesignTimeTenantId), Options.Create(behaviorOptions));
        }
    }

    /// <summary>
    /// 设计时租户提供者，返回固定的租户ID
    /// </summary>
    public class DesignTimeTenantProvider : ITenantProvider
    {
        private readonly long _tenantId;

        /// <summary>
        /// 构造函数，接受一个租户ID参数
        /// </summary>
        /// <param name="tenantId"></param>
        public DesignTimeTenantProvider(long tenantId)
        {
            _tenantId = tenantId;
        }

        public long TenantId => _tenantId;

        public void ClearTenantId()
        {
            throw new NotImplementedException();
        }

        public void SetTenantId(long tenantId)
        {
            throw new NotImplementedException();
        }
    }
}
