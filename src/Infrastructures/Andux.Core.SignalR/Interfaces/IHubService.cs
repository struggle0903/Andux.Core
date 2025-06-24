namespace Andux.Core.SignalR
{
    /// <summary>
    /// 聊天业务服务接口  (内存)
    /// </summary>
    public interface IHubService
    {
        /// <summary>
        /// 广播给所有用户
        /// </summary>
        /// <param name="user">用户</param>
        /// <param name="message">消息</param>
        /// <returns></returns>
        Task BroadcastAsync(string user, string message);

        /// <summary>
        /// 广播消息给所有其他客户端（排除当前发送者）
        /// </summary>
        /// <param name="connectionId">当前发送者的连接 ID</param>
        /// <param name="user">发送者名称</param>
        /// <param name="message">消息内容</param>
        Task BroadcastOthersAsync(string connectionId, string user, string message);

        /// <summary>
        /// 向指定用户发送消息
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="message">消息</param>
        /// <returns></returns>
        Task SendToUserAsync(string userId, string message);

        /// <summary>
        /// 向指定用户发送消息
        /// </summary>
        /// <param name="connectionId">连接id</param>
        /// <param name="message">消息</param>
        /// <returns></returns>
        Task SendToConnectionAsync(string connectionId, string message);

        /// <summary>
        /// 向指定组发送消息
        /// </summary>
        /// <param name="groupName">组名称</param>
        /// <param name="message">消息</param>
        /// <returns></returns>
        Task SendToGroupAsync(string groupName, string message);

        /// <summary>
        /// 添加到指定组（根据连接id）
        /// </summary>
        /// <param name="connectionId">连接id</param>
        /// <param name="groupName">组名</param>
        /// <returns></returns>
        Task JoinGroupAsync(string connectionId, string groupName);

        /// <summary>
        /// 移出指定组（根据连接id）
        /// </summary>
        /// <param name="connectionId">连接id</param>
        /// <param name="groupName">组名</param>
        /// <returns></returns>
        Task LeaveGroupAsync(string connectionId, string groupName);

        /// <summary>
        /// 添加到指定组（根据用户id）
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="groupName">组名</param>
        /// <returns></returns>
        Task JoinGroupByUserIdAsync(string userId, string groupName);

        /// <summary>
        /// 移出指定组（根据用户id）
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="groupName">组名</param>
        /// <returns></returns>
        Task LeaveGroupByUserIdAsync(string userId, string groupName);

        /// <summary>
        /// 删除链接（按连接id）
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        Task RemoveByConnectionIdAsync(string connectionId);

        /// <summary>
        /// 删除链接（按用户id）
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task RemoveByUserIdAsync(string userId);
    }
}
