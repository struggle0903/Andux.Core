namespace Andux.Core.SignalR.Interfaces
{
    /// <summary>
    /// 聊天业务服务接口，定义聊天相关操作。
    /// </summary>
    public interface IHubService
    {
        /// <summary>
        /// 向所有连接广播消息。
        /// </summary>
        Task BroadcastAsync(string user, string message);

        /// <summary>
        /// 向指定用户发送消息。
        /// </summary>
        Task SendToUserAsync(string connectionId, string message);

        /// <summary>
        /// 向指定组发送消息。
        /// </summary>
        Task SendToGroupAsync(string groupName, string message);

        ///// <summary>
        ///// 将用户加入指定组。
        ///// </summary>
        //Task JoinGroupAsync(string connectionId, string groupName);

        ///// <summary>
        ///// 将用户移出指定组。
        ///// </summary>
        //Task LeaveGroupAsync(string connectionId, string groupName);

        /// <summary>
        /// 删除链接 踢人
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        Task RemoveUserConnectionAsync(string connectionId);
    }
}
