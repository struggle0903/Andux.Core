using Microsoft.EntityFrameworkCore;

namespace Andux.Core.EfTenant
{
    /// <summary>
    /// 多租户 DbContext（分库模式）
    /// 支持运行时动态修改表名以实现租户数据隔离
    /// </summary>
    public class MultiTenantDbContext : DbContext
    {
        private readonly ITenantProvider _tenantProvider;
        private readonly bool _isDesignTime;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options">DbContext 配置选项</param>
        /// <param name="tenantProvider">租户提供器（设计时可传入 null）</param>
        protected MultiTenantDbContext(
            DbContextOptions options,
            ITenantProvider tenantProvider)
            : base(options)
        {
            _tenantProvider = tenantProvider;
        }

        /// <summary>
        /// 配置模型
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //// 设计时（如执行迁移）不修改表名，确保迁移生成的表结构正确
            //if (!IsUpdateDatabase())
            //{
            //    return;
            //}

            //// 运行时根据租户 ID 动态修改表名
            //if (_tenantProvider != null)
            //{
            //    try
            //    {
            //        var tenantId = _tenantProvider.TenantId;
            //        ApplyTenantTableNaming(modelBuilder, tenantId);
            //    }
            //    catch (Exception ex)
            //    {
            //        // 租户解析失败时使用默认表名，避免应用启动失败
            //        System.Diagnostics.Debug.WriteLine($"租户表名修改失败: {ex.Message}");
            //    }
            //}
        }

        /// <summary>
        /// 是否修改数据库
        /// </summary>
        /// <returns></returns>
        private static bool IsUpdateDatabase()
        {
            // 检查命令行参数中是否包含 update-database
            var args = Environment.GetCommandLineArgs();

            return args.Any(arg =>
                arg.Contains("update-database", StringComparison.OrdinalIgnoreCase) ||
                arg.Contains("database update", StringComparison.OrdinalIgnoreCase) ||
                (arg.Contains("ef") && args.Any(a => a.Contains("database") && a.Contains("update"))));
        }

        /// <summary>
        /// 应用租户表名规则
        /// </summary>
        /// <param name="modelBuilder">模型构建器</param>
        /// <param name="tenantId">租户 ID</param>
        private void ApplyTenantTableNaming(ModelBuilder modelBuilder, long tenantId)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // 跳过已显式配置表名的实体
                if (entityType.GetTableName() != entityType.GetDefaultTableName())
                {
                    continue;
                }

                var originalTableName = entityType.GetTableName();
                if (string.IsNullOrEmpty(originalTableName))
                {
                    continue;
                }

                // 生成租户隔离的表名：{原表名}_{租户ID}
                var tenantTableName = $"{originalTableName}_{tenantId}";
                entityType.SetTableName(tenantTableName);

                // 同时处理视图名称（如果存在）
                if (!string.IsNullOrEmpty(entityType.GetViewName()))
                {
                    entityType.SetViewName($"{entityType.GetViewName()}_{tenantId}");
                }
            }
        }

        /// <summary>
        /// 判断是否处于设计时（如执行迁移命令）
        /// 设计时不修改表名，避免生成错误的迁移脚本
        /// </summary>
        /// <returns>true 表示处于设计时，false 表示运行时</returns>
        private static bool IsDesignTime()
        {
            var designAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Select(a => a.FullName)
                .Where(fullName => !string.IsNullOrEmpty(fullName))
                .Any(fullName =>
                    fullName.Contains("Microsoft.EntityFrameworkCore.Design") ||
                    fullName.Contains("dotnet-ef") ||
                    fullName.Contains("ef"));

            return designAssemblies;
        }

    }
}
