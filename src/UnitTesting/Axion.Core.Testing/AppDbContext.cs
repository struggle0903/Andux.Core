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
        }

    }
}
