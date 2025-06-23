using Andux.Core.SignalR.Models;

namespace Andux.Core.SignalR.Interfaces
{
    /// <summary>
    /// 在线用户管理接口。
    /// </summary>
    public interface IUserConnectionManager
    {
        void AddConnection(OnlineUserInfo userInfo);
        void RemoveConnection(string connectionId);
        List<OnlineUserInfo> GetAllConnections();
        List<OnlineUserInfo> GetConnectionsByUserId(string userId);
        OnlineUserInfo? GetConnectionById(string connectionId);
    }
}
