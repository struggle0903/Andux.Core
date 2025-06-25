namespace Andux.Core.Testing.Model
{
    public class SendMultipleUsersRequest
    {
        /// <summary>
        /// 目标用户 ID 列表
        /// </summary>
        public List<string> UserIds { get; set; } = [];

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; set; } = string.Empty;
    }
}
