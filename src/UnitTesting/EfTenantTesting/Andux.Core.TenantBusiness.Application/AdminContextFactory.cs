using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Andux.Core.EfTenant;

namespace Andux.Core.TenantTesting.Application
{
    public class AdminContextFactory : IDesignTimeDbContextFactory<AdminContext>
    {
        public AdminContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var conn = config.GetConnectionString("Default");
            var optionsBuilder = new DbContextOptionsBuilder<AdminContext>();

            optionsBuilder.UseMySql(
                conn,
                ServerVersion.AutoDetect(conn));

            return new AdminContext(optionsBuilder.Options, new DesignTimeTenantProvider());
        }
    }

    public class DesignTimeTenantProvider : ITenantProvider
    {
        public long TenantId => 0;
    }
}
