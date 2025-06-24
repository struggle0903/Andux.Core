namespace Andux.Core.SignalR.Models
{
    /// <summary>
    /// 在线用户信息模型，用于统一管理用户连接。
    /// </summary>
    public class OnlineUserInfo
    {
        /// <summary>
        /// 连接 ID。
        /// </summary>
        public string ConnectionId { get; set; } = string.Empty;

        /// <summary>
        /// 用户唯一标识。
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 用户所属组集合。
        /// </summary>
        public List<string> Groups { get; set; } = [];

        /// <summary>
        /// 租户 ID（可选）。
        /// </summary>
        public string? TenantId { get; set; }

        /// <summary>
        /// 连接时间。
        /// </summary>
        public DateTime ConnectedAt { get; set; } = DateTime.Now;
    }
}
