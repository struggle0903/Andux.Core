// =======================================
// 作者：andy.hu
// 文件：AnduxDbContext.cs
// 描述：统一封装软删除逻辑的基础 DbContext
// =======================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Andux.Core.EfTrack
{
    /// <summary>
    /// 提供软删除过滤的基础 DbContext，可供具体上下文继承
    /// </summary>
    public abstract class AnduxDbContext : DbContext
    {
        private readonly EntityBehaviorOptions _options;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options"></param>
        /// <param name="behaviorOptions"></param>
        protected AnduxDbContext(DbContextOptions options, 
            IOptions<EntityBehaviorOptions> behaviorOptions)
            : base(options)
        {
            _options = behaviorOptions.Value;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 加软删除查询过滤器
            ApplySoftDeleteQueryFilter(modelBuilder);
        }

        /// <summary>
        /// 添加软删除查询过滤器
        /// </summary>
        private void ApplySoftDeleteQueryFilter(ModelBuilder modelBuilder)
        {
            if (!_options.EnableSoftDelete) return;

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var prop = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                    var condition = Expression.Equal(prop, Expression.Constant(false));
                    var lambda = Expression.Lambda(condition, parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            HandleSoftDelete();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            HandleSoftDelete();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// 处理软删除逻辑：将删除操作转换为修改状态并设置 IsDeleted 标志
        /// </summary>
        private void HandleSoftDelete()
        {
            if (!_options.EnableSoftDelete) return;

            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted))
            {
                if (entry.Entity is ISoftDelete softDelete)
                {
                    entry.State = EntityState.Modified;
                    softDelete.IsDeleted = true;
                }
            }
        }

        /// <summary>
        /// 注册实体
        /// </summary>
        /// <param name="modelBuilder"></param>
        /// <param name="assemblyMarkerTypes"></param>
        protected void RegisterEntities(ModelBuilder modelBuilder, params Type[] assemblyMarkerTypes)
        {
            var allTypes = assemblyMarkerTypes
                .SelectMany(t => t.Assembly.GetExportedTypes())
                .Where(t =>
                    t is { IsClass: true, IsAbstract: false } &&
                    t.GetInterfaces().Any(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>)))
                .Distinct();

            foreach (var type in allTypes)
            {
                modelBuilder.Entity(type);
            }
        }


        #region ---------- 原生 SQL / 批量操作支持 ----------

        /// <summary>
        /// 执行 SQL 查询并返回实体集合（支持异步）
        /// 用法示例:
        /// var users = await dbContext.FromSqlAsync<User>("SELECT * FROM Users WHERE Age > {0}", 18);
        /// </summary>
        public async Task<List<T>> FromSqlAsync<T>(string sql, params object[] parameters) where T : class
        {
            return await Set<T>().FromSqlRaw(sql, parameters).ToListAsync();
        }

        /// <summary>
        /// 执行原生 SQL 查询（无跟踪）
        /// 用法示例:
        /// var users = await dbContext.FromSqlNoTrackingAsync<User>("SELECT * FROM Users");
        /// </summary>
        public async Task<List<T>> FromSqlNoTrackingAsync<T>(string sql, params object[] parameters) where T : class
        {
            return await Set<T>().FromSqlRaw(sql, parameters).AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// 执行 SQL 命令（Insert/Update/Delete 等），返回受影响行数
        /// 用法示例:
        /// int rows = await dbContext.ExecuteSqlAsync("UPDATE Users SET Age = Age + 1 WHERE Id = {0}", userId);
        /// </summary>
        public async Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
        {
            return await Database.ExecuteSqlRawAsync(sql, parameters);
        }

        /// <summary>
        /// 批量插入（推荐小批量用 EF Core 的 AddRangeAsync，大批量用 EFCore.BulkExtensions）
        /// 用法示例:
        /// await dbContext.BulkInsertAsync(users);
        /// </summary>
        public async Task BulkInsertAsync<T>(IEnumerable<T> entities, int batchSize = 1000) where T : class
        {
            var list = entities.ToList();
            for (int i = 0; i < list.Count; i += batchSize)
            {
                var batch = list.Skip(i).Take(batchSize);
                await Set<T>().AddRangeAsync(batch);
                await SaveChangesAsync();
            }
        }

        /// <summary>
        /// 批量更新（通过 SQL 执行，避免逐条 Update）
        /// 用法示例:
        /// await dbContext.BulkUpdateAsync("UPDATE Users SET Age = {0} WHERE Age < {1}", 30, 18);
        /// </summary>
        public async Task<int> BulkUpdateAsync(string sql, params object[] parameters)
        {
            return await ExecuteSqlAsync(sql, parameters);
        }

        /// <summary>
        /// 批量删除（通过 SQL 执行）
        /// 用法示例:
        /// await dbContext.BulkDeleteAsync("DELETE FROM Users WHERE IsDeleted = 1");
        /// </summary>
        public async Task<int> BulkDeleteAsync(string sql, params object[] parameters)
        {
            return await ExecuteSqlAsync(sql, parameters);
        }

        /// <summary>
        /// 执行返回单个值的SQL标量查询（支持异步）
        /// </summary>
        public async Task<T> ExecuteScalarAsync<T>(string sql, params object[] parameters)
        {
            await using var command = Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            // 添加参数（防SQL注入）
            if (parameters is { Length: > 0 })
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = $"@p{i}";
                    parameter.Value = parameters[i] ?? DBNull.Value;
                    command.Parameters.Add(parameter);
                }
            }

            if (command.Connection.State != System.Data.ConnectionState.Open)
                await command.Connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();
            await command.Connection.CloseAsync();

            return (T)Convert.ChangeType(result, typeof(T));
        }

        #endregion


        //// 1. 原生 SQL 查询
        //var users = await dbContext.FromSqlAsync<User>("SELECT * FROM Users WHERE Age > {0}", 18);

        //// 2. 原生 SQL 更新
        //int rows = await dbContext.ExecuteSqlAsync("UPDATE Users SET Age = Age + 1 WHERE Id = {0}", userId);

        //// 3. 批量插入
        //await dbContext.BulkInsertAsync(new List<User>
        //{
        //    new User { Name = "张三", Age = 20 },
        //    new User { Name = "李四", Age = 25 }
        //});

        //// 4. 批量更新
        //await dbContext.BulkUpdateAsync("UPDATE Users SET IsActive = 0 WHERE LastLogin < {0}", DateTime.Now.AddYears(-1));

        //// 5. 批量删除
        //await dbContext.BulkDeleteAsync("DELETE FROM Users WHERE IsDeleted = 1");

    }
}
