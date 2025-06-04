using Axion.Core.EfTrack;
using EfFramework.Test.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Nevo.Core.Testing
{
    /// <summary>
    /// app 应用上下文
    /// </summary>
    public class AppDbContext : NovaDbContext
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
        }

    }
}
