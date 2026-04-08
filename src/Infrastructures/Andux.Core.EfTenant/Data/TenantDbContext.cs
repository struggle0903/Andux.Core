using Microsoft.EntityFrameworkCore;

namespace Andux.Core.EfTenant
{
    /// <summary>
    /// 平台租户库上下文接口（不分租户）
    /// </summary>
    public interface ITenantDbContext
    {
        DbSet<TenantDbConfig> TenantInfos { get; }
    }

    /// <summary>
    /// 平台租户库（不分租户）
    /// </summary>
    public class TenantDbContext : DbContext, ITenantDbContext
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options"></param>
        public TenantDbContext(DbContextOptions options)
            : base(options)
        {

        }

        /// <summary>
        /// 租户数据库配置信息表
        /// </summary>
        public DbSet<TenantDbConfig> TenantInfos => Set<TenantDbConfig>();

        /// <summary>
        /// 模型配置
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TenantDbConfig>(entity =>
            {
                entity.ToTable("sys_tenant_db_configs");

                entity.HasKey(x => x.TenantId);

                entity.Property(x => x.ConnectionString)
                    .IsRequired()
                    .HasMaxLength(500);
            });

            modelBuilder.Entity<TenantDbConfig>()
                .Property(p => p.Status)
                .HasConversion<string>();
        }

    }
}
