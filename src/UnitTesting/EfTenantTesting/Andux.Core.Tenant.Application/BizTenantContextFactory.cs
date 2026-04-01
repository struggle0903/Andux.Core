using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Andux.Core.Tenant.Application
{
    public class BizTenantContextFactory : IDesignTimeDbContextFactory<BizTenantContext>
    {
        public BizTenantContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            var conn = config.GetConnectionString("Tenant");
            var options = new DbContextOptionsBuilder<BizTenantContext>()
                .UseMySql(conn, new MySqlServerVersion(new Version(8, 0, 36)))
                .Options;

            return new BizTenantContext(options);
        }

    }
}