using Andux.Core.SignalR.Interfaces;
using Andux.Core.SignalR.Models;
using Microsoft.AspNetCore.Mvc;

namespace Andux.Core.Testing.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SignalRTestController : ControllerBase
    {
        private readonly IHubService _hubService;
        private readonly IUserConnectionManager _userManager;

        public SignalRTestController(IHubService hubService, IUserConnectionManager userManager)
        {
            _hubService = hubService;
            _userManager = userManager;
        }

        /// <summary>
        /// 测试广播消息
        /// </summary>
        [HttpPost("broadcast")]
        public async Task<IActionResult> Broadcast([FromBody] SignalRMessage msg)
        {
            await _hubService.BroadcastAsync(msg.Sender, msg.Content);
            return Ok("广播成功");
        }

        /// <summary>
        /// 测试发给指定连接
        /// </summary>
        [HttpPost("send-connection")]
        public async Task<IActionResult> SendToConnection(string connectionId, string content)
        {
            await _hubService.SendToUserAsync(connectionId, content);
            return Ok($"已发给连接 {connectionId}");
        }

        /// <summary>
        /// 获取当前所有在线用户
        /// </summary>
        [HttpGet("online-users")]
        public IActionResult GetOnlineUsers()
        {
            var users = _userManager.GetAllConnections();
            return Ok(users);
        }

        /// <summary>
        /// 踢掉指定连接
        /// </summary>
        [HttpPost("kick")]
        public async Task<IActionResult> KickUser(string connectionId)
        {
            await _hubService.RemoveUserConnectionAsync(connectionId);
            _userManager.RemoveConnection(connectionId);
            return Ok($"已踢掉连接 {connectionId}");
        }

        /// <summary>
        /// 向多个指定连接发送消息
        /// </summary>
        /// <param name="connectionIds">连接 ID 列表</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        [HttpPost("send-multiple-connections")]
        public async Task<IActionResult> SendToMultipleConnections([FromBody] SendMultipleRequest request)
        {
            foreach (var connId in request.ConnectionIds)
            {
                await _hubService.SendToUserAsync(connId, request.Content);
            }
            return Ok($"已向 {request.ConnectionIds.Count} 个连接发送消息");
        }

     
    }

    /// <summary>
    /// 请求模型：批量发消息
    /// </summary>
    public class SendMultipleRequest
    {
        /// <summary>
        /// 目标连接 ID 列表
        /// </summary>
        public List<string> ConnectionIds { get; set; } = new();

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; set; } = string.Empty;
    }
}
