using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Andux.Core.EfTenant
{
    /// <summary>
    /// 简单实现
    /// </summary>
    public class TenantStore : ITenantStore
    {
        private readonly TenantDbContext _db;
        private readonly IMemoryCache _cache;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="db"></param>
        /// <param name="cache"></param>
        public TenantStore(TenantDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        /// <summary>
        /// 获取租户信息
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<TenantDbConfig> GetAsync(long tenantId)
        {
            var tenant = await _db.Set<TenantDbConfig>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId);

            if (tenant == null)
                throw new Exception($"租户 {tenantId} 未找到");

            return tenant;
        }

        /// <summary>
        /// 获取租户连接字符串
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string GetConnectionString(long tenantId)
        {
            return _cache.GetOrCreate($"tenant_conn_{tenantId}", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                var config = _db.Set<TenantDbConfig>()
                    .AsNoTracking()
                    .FirstOrDefault(x => x.TenantId == tenantId);

                if (config == null || string.IsNullOrWhiteSpace(config.ConnectionString))
                {
                    throw new InvalidOperationException($"租户 {tenantId} 未配置数据库连接");
                }

                return config.ConnectionString;
            });
        }

    }
}
