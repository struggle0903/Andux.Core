using Andux.Core.EfTrack;
using Andux.Core.Testing.Entitys;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Andux.Core.Testing
{
    /// <summary>
    /// app 应用上下文
    /// </summary>
    public class AppDbContext : AnduxDbContext
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options"></param>
        /// <param name="behaviorOptions"></param>
        public AppDbContext(DbContextOptions<AppDbContext> options,
            IOptions<EntityBehaviorOptions> behaviorOptions)
            : base(options, behaviorOptions)
        {

        }

        /// <summary>
        /// 启用 Base 自动扫描
        /// </summary>
        protected override bool AutoRegisterEntities => false;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 实体注册
            modelBuilder.Entity<User>();
            modelBuilder.Entity<Role>();
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<Product>();
            modelBuilder.Entity<OrderItem>();

            //RegisterEntities(modelBuilder, typeof(TestUser));

            modelBuilder.Ignore<TestUser>();

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId);
        }

    }
}
