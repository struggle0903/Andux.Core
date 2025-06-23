using Andux.Core.SignalR.Interfaces;
using Andux.Core.SignalR.Models;

namespace Andux.Core.SignalR.Services
{
    /// <summary>
    /// 内存用户连接管理（可扩展为 Redis 等分布式实现）。
    /// </summary>
    public class UserConnectionManager : IUserConnectionManager
    {
        private readonly List<OnlineUserInfo> _connections = new();

        public void AddConnection(OnlineUserInfo userInfo)
        {
            lock (_connections)
            {
                _connections.Add(userInfo);
            }
        }

        public void RemoveConnection(string connectionId)
        {
            lock (_connections)
            {
                var target = _connections.FirstOrDefault(x => x.ConnectionId == connectionId);
                if (target != null)
                {
                    _connections.Remove(target);
                }
            }
        }

        public List<OnlineUserInfo> GetAllConnections()
        {
            lock (_connections)
            {
                return _connections.ToList();
            }
        }

        public List<OnlineUserInfo> GetConnectionsByUserId(string userId)
        {
            lock (_connections)
            {
                return _connections.Where(x => x.UserId == userId).ToList();
            }
        }

        public OnlineUserInfo? GetConnectionById(string connectionId)
        {
            lock (_connections)
            {
                return _connections.FirstOrDefault(x => x.ConnectionId == connectionId);
            }
        }
    }
}
