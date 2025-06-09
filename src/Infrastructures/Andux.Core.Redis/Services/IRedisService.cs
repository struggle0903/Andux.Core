// =======================================
// 作者：andy.hu
// 文件：IRedisService.cs
// 描述：提供统一的 Redis 操作接口封装，支持常用的字符串缓存读写、删除、判断、过期控制、自增自减、TTL 获取以及模式匹配 Key 搜索等功能，简化业务层对 Redis 的使用。
// =======================================

namespace Andux.Core.Redis.Services
{
    /// <summary>
    /// Redis 操作接口，定义常用方法
    /// </summary>
    public interface IRedisService
    {
        /// <summary>
        /// 设置缓存值（支持对象序列化）
        /// </summary>
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);

        /// <summary>
        /// 获取缓存值（自动反序列化）
        /// </summary>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// 删除缓存项
        /// </summary>
        Task<bool> RemoveAsync(string key);

        /// <summary>
        /// 判断 key 是否存在
        /// </summary>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// 自增 key 的数值
        /// </summary>
        Task<long> IncrementAsync(string key, long value = 1);

        /// <summary>
        /// 自减 key 的数值
        /// </summary>
        Task<long> DecrementAsync(string key, long value = 1);

        /// <summary>
        /// 设置 key 的过期时间
        /// </summary>
        Task<bool> ExpireAsync(string key, TimeSpan expiry);

        /// <summary>
        /// 获取 key 的剩余生存时间（TTL）
        /// </summary>
        Task<TimeSpan?> GetTtlAsync(string key);

        /// <summary>
        /// 根据模式匹配搜索 key（如 pattern:*）
        /// </summary>
        Task<IEnumerable<string>> SearchKeysAsync(string pattern);
    }
}
