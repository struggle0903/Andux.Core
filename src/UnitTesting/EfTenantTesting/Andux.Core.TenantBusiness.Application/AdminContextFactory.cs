using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Andux.Core.EfTenant;
using MySqlConnector;

namespace Andux.Core.TenantTesting.Application
{
    public class AdminContextFactory : IDesignTimeDbContextFactory<AdminContext>
    {
        // 需要执行迁移的租户id
        private const long tenantId = 1843473246985599999;

        public AdminContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var baseConn = config.GetConnectionString("Default");
            var builder = new MySqlConnectionStringBuilder(baseConn);
            builder.Database = $"{builder.Database}_{tenantId}";

            var conn = builder.ConnectionString;
            var options = new DbContextOptionsBuilder<AdminContext>()
                .UseMySql(conn, ServerVersion.AutoDetect(conn))
                .Options;

            return new AdminContext(options, new DesignTimeTenantProvider());
        }
    }

    public class DesignTimeTenantProvider : ITenantProvider
    {
        public long TenantId => 1843473246985599999;
    }
}
