using Andux.Core.SignalR.Clients;
using Andux.Core.SignalR.Interfaces;
using Andux.Core.SignalR.Models;
using Andux.Core.Testing.Controllers.Base;
using Andux.Core.Testing.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Andux.Core.Testing.Controllers
{
    public class RedisSignalRController : ApiBaseController
    {
        private readonly IRedisHubService _hubService;
        private readonly IRedisUserConnectionManager _redisUserManager;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="hubService"></param>
        /// <param name="redisUserManager"></param>
        public RedisSignalRController(IRedisHubService hubService,
            IRedisUserConnectionManager redisUserManager)
        {
            _hubService = hubService;
            _redisUserManager = redisUserManager;
        }

        /// <summary>
        /// 测试广播消息
        /// </summary>
        [Authorize]
        [HttpPost("createClient")]
        public async Task<IActionResult> CreateClient(string token)
        {
            var client = new SignalRClient("http://127.0.0.1:5001/chatHub", token);

            client.On<string, string>("ReceiveMessage", (user, msg) =>
            {
                Console.WriteLine($"{user}: {msg}");
            });

            await client.ConnectAsync();
            await client.SendMessageAsync("SendMessage", "客户端用户", "你好 SignalR");
            return Ok("广播成功");
        }

        /// <summary>
        /// 测试广播消息给所有用户
        /// </summary>
        [HttpPost("broadcast")]
        public async Task<IActionResult> Broadcast([FromBody] SignalRMessage msg)
        {
            await _hubService.BroadcastAsync(msg.Sender, msg.Content);
            return Ok("广播成功");
        }

        /// <summary>
        /// 测试广播消息给除本人的其他用户
        /// </summary>
        [HttpPost("broadcastOthers")]
        public async Task<IActionResult> BroadcastOthers(string connectionId, [FromBody] SignalRMessage msg)
        {
            await _hubService.BroadcastOthersAsync(connectionId, msg.Sender, msg.Content);
            return Ok("广播成功");
        }

        /// <summary>
        /// 测试发给指定连接
        /// </summary>
        [HttpPost("send-connection")]
        public async Task<IActionResult> SendToConnection(string connectionId, string content)
        {
            await _hubService.SendToConnectionAsync(connectionId, content);
            return Ok($"已发给连接 {connectionId}");
        }

        /// <summary>
        /// 测试发给指定用户
        /// </summary>
        [HttpPost("send-user")]
        public async Task<IActionResult> SendToUser(string userId, string content)
        {
            await _hubService.SendToUserAsync(userId, content);
            return Ok($"已发给用户 {userId}");
        }

        /// <summary>
        /// 获取当前所有在线用户
        /// </summary>
        [HttpGet("online-users")]
        public IActionResult GetOnlineUsers()
        {
            var users = _redisUserManager.GetAllConnections();
            return Ok(users);
        }

        /// <summary>
        /// 踢掉指定连接
        /// </summary>
        [HttpPost("kick")]
        public async Task<IActionResult> Kick(string connectionId)
        {
            await _hubService.RemoveByConnectionIdAsync(connectionId);
            return Ok($"已踢掉连接 {connectionId}");
        }

        /// <summary>
        /// 踢掉指定用户
        /// </summary>
        [HttpPost("kickByUser")]
        public async Task<IActionResult> KickUser(string userId)
        {
            await _hubService.RemoveByUserIdAsync(userId);
            return Ok($"已踢掉用户 {userId}");
        }

        /// <summary>
        /// 向多个指定连接发送消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("send-multiple-connections")]
        public async Task<IActionResult> SendToMultipleConnections([FromBody] SendMultipleConnectionsRequest request)
        {
            foreach (var connId in request.ConnectionIds)
            {
                await _hubService.SendToConnectionAsync(connId, request.Content);
            }
            return Ok($"已向 {request.ConnectionIds.Count} 个连接发送消息");
        }

        /// <summary>
        /// 向多个指定连接发送消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("send-multiple-user")]
        public async Task<IActionResult> SendToMultipleUsers([FromBody] SendMultipleUsersRequest request)
        {
            foreach (var connId in request.UserIds)
            {
                await _hubService.SendToUserAsync(connId, request.Content);
            }
            return Ok($"已向 {request.UserIds.Count} 个用户发送消息");
        }

        /// <summary>
        /// 加入指定组
        /// </summary>
        [HttpPost("sendToGroup")]
        public async Task<IActionResult> SendToGroup(string groupName, string message)
        {
            await _hubService.SendToGroupAsync(groupName, message);
            return Ok($"已向组{groupName}发生消息 {message}");
        }

        /// <summary>
        /// 加入指定组
        /// </summary>
        [HttpPost("joinGroup")]
        public async Task<IActionResult> JoinGroup(string connectionId, string groupName)
        {
            await _hubService.JoinGroupAsync(connectionId, groupName);
            return Ok($"连接{connectionId}已加入到组 {groupName}");
        }

        /// <summary>
        /// 移出指定组
        /// </summary>
        [HttpPost("leaveGroup")]
        public async Task<IActionResult> LeaveGroup(string connectionId, string groupName)
        {
            await _hubService.LeaveGroupAsync(connectionId, groupName);
            return Ok($"已将连接{connectionId}移除组 {groupName}");
        }

        /// <summary>
        /// 加入指定组
        /// </summary>
        [HttpPost("joinGroupByUserId")]
        public async Task<IActionResult> JoinGroupByUserId(string userId, string groupName)
        {
            await _hubService.JoinGroupByUserIdAsync(userId, groupName);
            return Ok($"用户{userId}已加入到组 {groupName}");
        }

        /// <summary>
        /// 移出指定组
        /// </summary>
        [HttpPost("leaveGroupByUserId")]
        public async Task<IActionResult> LeaveGroupByUserId(string userId, string groupName)
        {
            await _hubService.LeaveGroupByUserIdAsync(userId, groupName);
            return Ok($"用户 {userId}移除组 {groupName}");
        }

        /// <summary>
        /// 判断指定用户当前是否在线（至少有一个连接）
        /// </summary>
        [HttpPost("checkOnline")]
        public async Task<IActionResult> IsOnline(string userId)
        {
            var isOnline = _redisUserManager.IsOnline(userId);
            var status = isOnline ? "在线" : "离线";
            return Ok($"用户 {userId} 当前状态 {status}");
        }

        /// <summary>
        /// 获取指定用户的所有连接ID列表
        /// </summary>
        [HttpPost("getConnectionIds")]
        public async Task<IActionResult> GetConnectionIdsByUserId(string userId)
        {
            var list = _redisUserManager.GetConnectionIdsByUserId(userId);
            return Ok($"用户 {userId} 存在连接id有 {string.Join(",", list.Select(x => $"'{x}'"))}");
        }

        /// <summary>
        /// 获取属于指定群组的所有连接信息列表。
        /// 一个连接可以属于多个群组。
        /// </summary>
        [HttpPost("getConnectionsByGroup")]
        public async Task<IActionResult> GetConnectionsByGroup(string groupName)
        {
            List<OnlineUserInfo> list = _redisUserManager.GetConnectionsByGroup(groupName);
            return Ok(list);
        }

        /// <summary>
        /// 获取指定租户下所有在线连接信息
        /// </summary>
        [HttpPost("getConnectionsByTenant")]
        public async Task<IActionResult> GetConnectionsByTenant(string tenantId)
        {
            List<OnlineUserInfo> list = _redisUserManager.GetConnectionsByTenant(tenantId);
            return Ok(list);
        }

        /// <summary>
        /// 清空所有连接信息，通常用于调试或服务重启时清理状态
        /// </summary>
        [HttpPost("clearAll")]
        public async Task<IActionResult> ClearAll()
        {
            _redisUserManager.ClearAll();
            return Ok($"已清空所有连接信息");
        }

    }
}
