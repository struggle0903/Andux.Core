namespace Andux.Core.SignalR
{
    /// <summary>
    /// SignalR 配置选项。
    /// </summary>
    public class SignalROptions
    {
        /// <summary>
        /// Redis 分布式连接串（为空则不启用 Redis）。
        /// </summary>
        public string? RedisConnection { get; set; }
    }
}
