
namespace Andux.Core.SignalR.Models
{
    /// <summary>
    /// 通用 SignalR 消息模型。
    /// </summary>
    public class SignalRMessage
    {
        public string Sender { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Group { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
