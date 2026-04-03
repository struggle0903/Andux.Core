using Andux.Core.EfTenant;
using Andux.Core.TenantTesting.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace Andux.Core.EfTenantTesting
{
    [TestClass]
    public sealed class InitTenantTenant
    {
        private TenantDbContext _tenantDb = null!;
        private IConfiguration _configuration = null!;

        [TestInitialize]
        public void Setup()
        {
            // 构建配置
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables() // 支持环境变量覆盖
                .Build();

            var conn = _configuration.GetConnectionString("Default");

            var options = new DbContextOptionsBuilder<TenantDbContext>()
                .UseMySql(conn, new MySqlServerVersion(new Version(8, 0, 36)))
                .Options;

            _tenantDb = new TenantDbContext(options);
        }

        /// <summary>
        /// 测试方法：创建租户数据库并保存租户信息
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Should_Create_Tenant()
        {
            var tenantId = long.Parse(_configuration.GetConnectionString("DefaultTenantId") ?? "0");

            try
            {
                // 1️ 获取基础连接字符串（不带数据库）
                var baseConn = _tenantDb.Database.GetConnectionString();

                // 2️ 创建数据库
                var tenantDbName = await CreateTenantDatabase(baseConn, tenantId);

                // 3️ 构建租户连接字符串
                var tenantConn = BuildTenantConnectionString(baseConn, tenantDbName);

                // 4️ 执行迁移（建表）
                await MigrateTenantDb(tenantConn, tenantId);

                // 5️ 保存租户信息
                if (!await _tenantDb.TenantInfos.AnyAsync(a => a.TenantId == tenantId))
                    _tenantDb.TenantInfos.Add(new TenantDbConfig
                    {
                        Id = tenantId, // 改雪花算法id
                        TenantId = tenantId,
                        Status = TenantStatusEnum.Active,
                        ConnectionString = tenantConn,
                        Remark = "单元测试生成"
                    });

                await _tenantDb.SaveChangesAsync();
            }
            catch(Exception ex)
            {

            }
        }

        /// <summary>
        /// 创建数据库
        /// </summary>
        private async Task<string> CreateTenantDatabase(string baseConn, long tenantId)
        {
            var builder = new MySqlConnectionStringBuilder(baseConn);

            // 拼接租户数据库名，格式：原数据库名_租户ID
            var dbName = builder.Database += $"_{tenantId}";

            // 去掉数据库名（关键）
            builder.Database = "";

            using var conn = new MySqlConnection(builder.ConnectionString);
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = $"CREATE DATABASE IF NOT EXISTS `{dbName}`;";
            await cmd.ExecuteNonQueryAsync();

            return dbName;
        }

        /// <summary>
        /// 拼接租户连接字符串
        /// </summary>
        private string BuildTenantConnectionString(string baseConn, string dbName)
        {
            var builder = new MySqlConnectionStringBuilder(baseConn);
            builder.Database = dbName;
            return builder.ConnectionString;
        }

        /// <summary>
        /// 按连接字符串执行 EF Migration
        /// </summary>
        private async Task MigrateTenantDb(string conn, long tenantId)
        {
            var options = new DbContextOptionsBuilder<AdminContext>()
                .UseMySql(conn, new MySqlServerVersion(new Version(8, 0, 36)))
                .Options;

            var behaviorOptions = new EntityBehaviorOptions { EnableSoftDelete = false };
            using var db = new AdminContext(options, new DesignTimeTenantProvider(tenantId), Options.Create(behaviorOptions));

            await db.Database.MigrateAsync();
        }

    }
}
