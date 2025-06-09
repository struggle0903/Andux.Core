// =======================================
// 作者：andy.hu
// 文件：RedisService.cs
// 描述：IRedisService 相关接口的实现。
// =======================================

using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace Andux.Core.Redis.Services
{
    /// <summary>
    /// Redis 服务实现
    /// </summary>
    public class RedisService : IRedisService
    {
        private readonly IDatabase _db;
        private readonly IConnectionMultiplexer _muxer;
        private readonly AnduxRedisOptions _options;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="muxer"></param>
        /// <param name="options"></param>
        public RedisService(IConnectionMultiplexer muxer, IOptions<AnduxRedisOptions> options)
        {
            _muxer = muxer;
            _options = options.Value;
            _db = muxer.GetDatabase(_options.DefaultDatabase);
        }

        private string BuildKey(string key) => string.IsNullOrEmpty(_options.InstanceName) ? key : $"{_options.InstanceName}:{key}";

        /// <summary>
        /// 设置缓存值（支持对象序列化）
        /// </summary>
        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(value);
            return await _db.StringSetAsync(BuildKey(key), json, expiry);
        }

        /// <summary>
        /// 获取缓存值（自动反序列化）
        /// </summary>
        public async Task<T?> GetAsync<T>(string key)
        {
            var val = await _db.StringGetAsync(BuildKey(key));
            return val.HasValue ? JsonSerializer.Deserialize<T>(val!) : default;
        }

        /// <summary>
        /// 删除缓存项
        /// </summary>
        public async Task<bool> RemoveAsync(string key)
            => await _db.KeyDeleteAsync(BuildKey(key));

        /// <summary>
        /// 判断 key 是否存在
        /// </summary>
        public async Task<bool> ExistsAsync(string key)
            => await _db.KeyExistsAsync(BuildKey(key));

        /// <summary>
        /// 自增 key 的数值
        /// </summary>
        public async Task<long> IncrementAsync(string key, long value = 1)
            => await _db.StringIncrementAsync(BuildKey(key), value);

        /// <summary>
        /// 自减 key 的数值
        /// </summary>
        public async Task<long> DecrementAsync(string key, long value = 1)
            => await _db.StringDecrementAsync(BuildKey(key), value);

        /// <summary>
        /// 设置 key 的过期时间
        /// </summary>
        public async Task<bool> ExpireAsync(string key, TimeSpan expiry)
            => await _db.KeyExpireAsync(BuildKey(key), expiry);

        /// <summary>
        /// 获取 key 的剩余生存时间（TTL）
        /// </summary>
        public async Task<TimeSpan?> GetTtlAsync(string key)
            => await _db.KeyTimeToLiveAsync(BuildKey(key));

        /// <summary>
        /// 根据模式匹配搜索 key（如 pattern:*）
        /// </summary>
        public async Task<IEnumerable<string>> SearchKeysAsync(string pattern)
        {
            var endpoints = _muxer.GetEndPoints();
            var server = _muxer.GetServer(endpoints.First());
            return server.Keys(database: _options.DefaultDatabase, pattern: BuildKey(pattern)).Select(k => k.ToString());
        }

    }
}
