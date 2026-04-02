using Andux.Core.EfTenant;
using Andux.Core.TenantTesting.Application.Entitys;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Andux.Core.TenantTesting.Application
{
    /// <summary>
    /// AdminContext
    /// </summary>
    public class AdminContext : MultiTenantDbContext
    {
        public AdminContext(DbContextOptions<AdminContext> options,
            ITenantProvider tenantProvider,
            IOptions<EntityBehaviorOptions> behaviorOptions)
            : base(options, tenantProvider, behaviorOptions)
        {

        }

        public DbSet<User> Users => Set<User>();
    }
}
