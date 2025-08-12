using Andux.Admin.Domain.Entitys;
using Andux.Core.EfTrack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Andux.Admin.Domain
{
    public class AnduxAdminDbContext: AnduxDbContext
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options"></param>
        /// <param name="behaviorOptions"></param>
        public AnduxAdminDbContext(DbContextOptions<AnduxAdminDbContext> options,
            IOptions<EntityBehaviorOptions> behaviorOptions) :
            base(options, behaviorOptions)
        {

        }

        /// <summary>
        /// 模型创建事件
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            RegisterEntities(modelBuilder, typeof(AnduxUser));
            RegisterEntities(modelBuilder, typeof(AnduxRole));
            RegisterEntities(modelBuilder, typeof(AnduxUserRole));
            RegisterEntities(modelBuilder, typeof(AnduxPermission));
            RegisterEntities(modelBuilder, typeof(AnduxRolePermission));

            modelBuilder.Entity<AnduxPermission>()
                .Property(p => p.Type)
                .HasConversion<string>();

            modelBuilder.Entity<AnduxUserRole>(b =>
            {
                b.HasKey(ur => new { ur.UserId, ur.RoleId });
                b.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId);
                b.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId);
                b.HasIndex(ur => ur.RoleId); // 查询角色下用户
            });

            modelBuilder.Entity<AnduxRolePermission>(b =>
            {
                b.HasKey(rp => new { rp.RoleId, rp.PermissionId });
                b.HasOne(rp => rp.Role).WithMany(r => r.RolePermissions).HasForeignKey(rp => rp.RoleId);
                b.HasOne(rp => rp.Permission).WithMany(p => p.RolePermissions).HasForeignKey(rp => rp.PermissionId);
                b.HasIndex(rp => rp.PermissionId); // 查询使用某权限的角色
            });

            base.OnModelCreating(modelBuilder);
        }

    }
}
