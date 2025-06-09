// =======================================
// 作者：andy.hu
// 文件：RedisHelper.cs
// 描述：Redis 操作静态辅助类，用于全局访问 RedisService 实例。适用于不方便通过依赖注入获取 IRedisService 的场景。
// =======================================

using Andux.Core.Redis.Services;

namespace Andux.Core.Redis.Helper
{
    /// <summary>
    /// Redis 操作静态辅助类，用于全局访问 RedisService 实例。
    /// 适用于不方便通过依赖注入获取 IRedisService 的场景。
    /// </summary>
    public static class RedisHelper
    {
        /// <summary>
        /// 当前全局 Redis 服务实例。
        /// 在程序启动时需通过 Configure 方法注入具体实现。
        /// </summary>
        public static IRedisService? Instance { get; private set; }

        /// <summary>
        /// 配置 RedisHelper 使用的 Redis 服务实例。
        /// 该方法通常在应用启动时调用，仅需调用一次。
        /// </summary>
        /// <param name="service">实现了 IRedisService 的 Redis 服务实例</param>
        public static void Configure(IRedisService service)
        {
            Instance = service ?? throw new ArgumentNullException(nameof(service));
        }
    }
}
