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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 实体注册
            RegisterEntities(modelBuilder, typeof(User));
            RegisterEntities(modelBuilder, typeof(Role));

            RegisterEntities(modelBuilder, typeof(Customer));
            RegisterEntities(modelBuilder, typeof(Order));
            RegisterEntities(modelBuilder, typeof(Product));
            RegisterEntities(modelBuilder, typeof(OrderItem));


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
