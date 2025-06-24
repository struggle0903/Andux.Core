using Andux.Core.SignalR.Hubs;
using Andux.Core.SignalR.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Andux.Core.SignalR.Services
{
    /// <summary>
    /// SignalR 消息推送业务实现 (redis)
    /// </summary>
    public class RedisHubService: IRedisHubService
    {
        private readonly IHubContext<AnduxRedisChatHub> _hubContext;
        private readonly IRedisUserConnectionManager _redisUserConnectionManager;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="hubContext"></param>
        /// <param name="redisUserConnectionManager"></param>
        public RedisHubService(IHubContext<AnduxRedisChatHub> hubContext
            , IRedisUserConnectionManager redisUserConnectionManager)
        {
            _hubContext = hubContext;
            _redisUserConnectionManager = redisUserConnectionManager;
        }

        /// <summary>
        /// 广播给所有用户
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task BroadcastAsync(string user, string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        /// <summary>
        /// 删除链接（按用户id）
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task RemoveByUserIdAsync(string userId)
        {
            var connections = _redisUserConnectionManager.GetConnectionsByUserId(userId);
            foreach (var conn in connections)
            {
                await _hubContext.Clients.Client(conn.ConnectionId).SendAsync("ForceDisconnect");
                _redisUserConnectionManager.RemoveConnection(conn.ConnectionId);
            }
        }

        /// <summary>
        /// 向指定用户发送消息
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="message">消息</param>
        /// <returns></returns>
        public async Task SendToUserAsync(string userId, string message)
        {
            var connections = _redisUserConnectionManager.GetConnectionsByUserId(userId);
            foreach (var conn in connections)
            {
                await _hubContext.Clients.Client(conn.ConnectionId).SendAsync("ReceiveMessage", "系统", message);
            }
        }

        /// <summary>
        /// 广播消息给所有其他客户端（排除当前发送者）
        /// </summary>
        /// <param name="connectionId">当前发送者的连接 ID</param>
        /// <param name="user">发送者名称</param>
        /// <param name="message">消息内容</param>
        public async Task BroadcastOthersAsync(string connectionId, string user, string message)
        {
            await _hubContext.Clients.AllExcept(connectionId).SendAsync("ReceiveMessage", user, message);
        }

        /// <summary>
        /// 向指定用户发送消息
        /// </summary>
        /// <param name="connectionId">连接id</param>
        /// <param name="message">消息</param>
        /// <returns></returns>
        public async Task SendToConnectionAsync(string connectionId, string message)
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", "系统", message);
        }

        /// <summary>
        /// 删除链接（按连接id）
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public async Task RemoveByConnectionIdAsync(string connectionId)
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("ForceDisconnect");
            _redisUserConnectionManager.RemoveConnection(connectionId);
        }

        #region 组操作
        /// <summary>
        /// 向指定组发送消息
        /// </summary>
        /// <param name="groupName">组名称</param>
        /// <param name="message">消息</param>
        /// <returns></returns>
        public async Task SendToGroupAsync(string groupName, string message)
        {
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", "系统", message);
        }

        /// <summary>
        /// 添加到指定组（根据连接id）
        /// </summary>
        /// <param name="connectionId">连接id</param>
        /// <param name="groupName">组名</param>
        /// <returns></returns>
        public async Task JoinGroupAsync(string connectionId, string groupName)
        {
            await _hubContext.Groups.AddToGroupAsync(connectionId, groupName);
            SyncGroups(connectionId, groupName);
        }

        /// <summary>
        /// 移出指定组（根据连接id）
        /// </summary>
        /// <param name="connectionId">连接id</param>
        /// <param name="groupName">组名</param>
        /// <returns></returns>
        public async Task LeaveGroupAsync(string connectionId, string groupName)
        {
            await _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);
            SyncGroups(connectionId, groupName);
        }

        /// <summary>
        /// 添加到指定组（根据用户id）
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="groupName">组名</param>
        /// <returns></returns>
        public async Task JoinGroupByUserIdAsync(string userId, string groupName)
        {
            var connections = _redisUserConnectionManager.GetConnectionsByUserId(userId);
            foreach (var conn in connections)
            {
                await _hubContext.Groups.AddToGroupAsync(conn.ConnectionId, groupName);
                SyncGroups(conn.ConnectionId, groupName);
            }
        }

        /// <summary>
        /// 移出指定组（根据用户id）
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="groupName">组名</param>
        /// <returns></returns>
        public async Task LeaveGroupByUserIdAsync(string userId, string groupName)
        {
            var connections = _redisUserConnectionManager.GetConnectionsByUserId(userId);
            foreach (var conn in connections)
            {
                await _hubContext.Groups.RemoveFromGroupAsync(conn.ConnectionId, groupName);
                SyncGroups(conn.ConnectionId, groupName);
            }
        }
        #endregion

        #region 私有方法

        /// <summary>
        /// 同步分组数据
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="groupName"></param>
        public void SyncGroups(string connectionId, string groupName)
        {
            // 同步Groups
            var user = _redisUserConnectionManager.GetConnectionById(connectionId);
            if (user != null && !user.Groups.Contains(groupName))
            {
                user.Groups.Add(groupName);
            }
        }

        #endregion

    }
}
