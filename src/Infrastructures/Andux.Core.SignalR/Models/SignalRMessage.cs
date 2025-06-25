namespace Andux.Core.SignalR.Models
{
    /// <summary>
    /// 通用 SignalR 消息模型。
    /// </summary>
    public class SignalRMessage
    {
        /// <summary>
        /// 发送方标识（用户名、用户ID、系统等）。
        /// </summary>
        public string Sender { get; set; } = string.Empty;

        /// <summary>
        /// 消息内容正文。
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 消息所属群组名称（可选）。为空表示是广播或点对点消息。
        /// </summary>
        public string? Group { get; set; }

        /// <summary>
        /// 消息发送时间（UTC时间），用于前端展示或排序。
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
