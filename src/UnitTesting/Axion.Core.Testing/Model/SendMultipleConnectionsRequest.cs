namespace Andux.Core.Testing.Model
{
    /// <summary>
    /// 请求模型：批量发消息
    /// </summary>
    public class SendMultipleConnectionsRequest
    {
        /// <summary>
        /// 目标连接 ID 列表
        /// </summary>
        public List<string> ConnectionIds { get; set; } = [];

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; set; } = string.Empty;
    }
}
