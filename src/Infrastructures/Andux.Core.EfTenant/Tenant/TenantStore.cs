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
            return _cache.GetOrCreate(tenantId, entry =>
            {
                // 设置缓存过期时间为10分钟
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                var config = _db.Set<TenantDbConfig>()
                    .AsNoTracking()
                    .FirstOrDefault(x => x.TenantId == tenantId);

                if (config == null)
                    throw new Exception($"未找到租户ID（{tenantId}）的db配置");

                return config.ConnectionString;
            });
        }

    }
}
