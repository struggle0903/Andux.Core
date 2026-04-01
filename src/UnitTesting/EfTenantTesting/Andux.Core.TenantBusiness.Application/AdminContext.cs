using Andux.Core.EfTenant;
using Andux.Core.TenantTesting.Application.Entitys;
using Microsoft.EntityFrameworkCore;

namespace Andux.Core.TenantTesting.Application
{
    /// <summary>
    /// AdminContext
    /// </summary>
    public class AdminContext : MultiTenantDbContext
    {
        public AdminContext(DbContextOptions<AdminContext> options,
            ITenantProvider tenantProvider)
            : base(options, tenantProvider)
        {

        }

        public DbSet<User> Users => Set<User>();
    }
}
