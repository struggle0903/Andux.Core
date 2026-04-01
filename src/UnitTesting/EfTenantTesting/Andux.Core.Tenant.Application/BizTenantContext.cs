using Andux.Core.EfTenant;
using Microsoft.EntityFrameworkCore;

namespace Andux.Core.Tenant.Application
{
    /// <summary>
    /// 业务租户 DbContext
    /// </summary>
    public class BizTenantContext: TenantDbContext
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options"></param>
        public BizTenantContext(DbContextOptions<BizTenantContext> options)
           : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

    }
}
