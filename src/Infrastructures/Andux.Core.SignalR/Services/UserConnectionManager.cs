using Andux.Core.SignalR.Interfaces;
using Andux.Core.SignalR.Models;

namespace Andux.Core.SignalR.Services
{
    /// <summary>
    /// 内存用户连接管理
    /// </summary>
    public class UserConnectionManager : IUserConnectionManager
    {
        /// <summary>
        /// 存储所有在线用户连接信息的列表。
        /// </summary>
        private readonly List<OnlineUserInfo> _connections = [];

        /// <summary>
        /// 注册连接
        /// </summary>
        /// <param name="userInfo"></param>
        public void AddConnection(OnlineUserInfo userInfo)
        {
            lock (_connections)
            {
                _connections.Add(userInfo);
            }
        }

        /// <summary>
        /// 移除连接
        /// </summary>
        /// <param name="connectionId"></param>
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

        /// <summary>
        /// 获取所有在线用户
        /// </summary>
        /// <returns></returns>
        public List<OnlineUserInfo> GetAllConnections()
        {
            lock (_connections)
            {
                return _connections.ToList();
            }
        }

        /// <summary>
        /// 根据用户ID获取多个连接
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<OnlineUserInfo> GetConnectionsByUserId(string userId)
        {
            lock (_connections)
            {
                return _connections.Where(x => x.UserId == userId).ToList();
            }
        }

        /// <summary>
        /// 根据连接ID获取连接对象
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public OnlineUserInfo? GetConnectionById(string connectionId)
        {
            lock (_connections)
            {
                return _connections.FirstOrDefault(x => x.ConnectionId == connectionId);
            }
        }

        /// <summary>
        /// 判断用户是否在线
        /// </summary>
        public bool IsOnline(string userId)
        {
            lock (_connections)
            {
                return _connections.Any(x => x.UserId == userId);
            }
        }

        /// <summary>
        /// 获取指定用户的所有连接ID
        /// </summary>
        public List<string> GetConnectionIdsByUserId(string userId)
        {
            lock (_connections)
            {
                return _connections
                    .Where(x => x.UserId == userId)
                    .Select(x => x.ConnectionId)
                    .ToList();
            }
        }

        /// <summary>
        /// 获取属于指定群组的连接列表
        /// </summary>
        public List<OnlineUserInfo> GetConnectionsByGroup(string groupName)
        {
            lock (_connections)
            {
                return _connections
                    .Where(x => x.Groups.Contains(groupName))
                    .ToList();
            }
        }

        /// <summary>
        /// 获取指定租户下所有在线用户
        /// </summary>
        public List<OnlineUserInfo> GetConnectionsByTenant(string tenantId)
        {
            lock (_connections)
            {
                return _connections
                    .Where(x => x.TenantId == tenantId)
                    .ToList();
            }
        }

        /// <summary>
        /// 清空所有连接（用于调试或重启）
        /// </summary>
        public void ClearAll()
        {
            lock (_connections)
            {
                _connections.Clear();
            }
        }

    }
}
