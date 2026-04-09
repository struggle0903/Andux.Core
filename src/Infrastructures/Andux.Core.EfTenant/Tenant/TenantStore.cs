using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Andux.Core.EfTenant
{
    /// <summary>
    /// 简单实现
    /// </summary>
    public class TenantStore : ITenantStore
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCache _cache;
        private readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);

        public TenantStore(IServiceProvider serviceProvider, IMemoryCache cache)
        {
            _serviceProvider = serviceProvider;
            _cache = cache;
        }

        public async Task<TenantDbConfig> GetAsync(long tenantId)
        {
            var cacheKey = $"tenant_config_{tenantId}";

            if (!_cache.TryGetValue(cacheKey, out TenantDbConfig? config))
            {
                await _cacheLock.WaitAsync();
                try
                {
                    if (!_cache.TryGetValue(cacheKey, out config))
                    {
                        // 在需要时创建 DbContext 作用域
                        using var scope = _serviceProvider.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

                        config = await db.Set<TenantDbConfig>()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.TenantId == tenantId);

                        if (config == null)
                            throw new Exception($"租户 {tenantId} 未找到");

                        _cache.Set(cacheKey, config, new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                            Priority = CacheItemPriority.High
                        });
                    }
                }
                finally
                {
                    _cacheLock.Release();
                }
            }

            return config!;
        }

        public string GetConnectionString(long tenantId)
        {
            var cacheKey = $"tenant_conn_{tenantId}";

            return _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                entry.SetPriority(CacheItemPriority.High);

                // 在需要时创建 DbContext 作用域
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

                var config = db.Set<TenantDbConfig>()
                    .AsNoTracking()
                    .FirstOrDefault(x => x.TenantId == tenantId);

                if (config == null || string.IsNullOrWhiteSpace(config.ConnectionString))
                {
                    throw new InvalidOperationException($"租户 {tenantId} 未配置数据库连接");
                }

                return config.ConnectionString;
            })!;
        }

    }
}
