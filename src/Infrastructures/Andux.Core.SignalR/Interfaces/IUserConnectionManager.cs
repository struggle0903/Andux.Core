using Andux.Core.SignalR.Models;

namespace Andux.Core.SignalR
{
    /// <summary>
    /// 在线用户管理接口 (内存)
    /// </summary>
    public interface IUserConnectionManager
    {
        /// <summary>
        /// 注册一个新的连接信息，添加到在线连接管理中。
        /// 通常在用户连接建立时调用，记录连接ID、用户ID、所属组、租户等信息。
        /// </summary>
        /// <param name="userInfo">包含连接相关信息的用户连接对象。</param>
        void AddConnection(OnlineUserInfo userInfo);

        /// <summary>
        /// 移除指定连接ID的连接信息，通常在连接断开时调用。
        /// </summary>
        /// <param name="connectionId">要移除的连接ID。</param>
        void RemoveConnection(string connectionId);

        /// <summary>
        /// 获取当前所有在线的连接列表，包含所有用户的所有连接。
        /// </summary>
        /// <returns>返回所有在线连接信息的列表。</returns>
        List<OnlineUserInfo> GetAllConnections();

        /// <summary>
        /// 根据用户ID获取该用户所有的连接信息列表。
        /// 一个用户可能有多个连接（比如多端登录）。
        /// </summary>
        /// <param name="userId">用户的唯一标识ID。</param>
        /// <returns>该用户所有连接信息列表。</returns>
        List<OnlineUserInfo> GetConnectionsByUserId(string userId);

        /// <summary>
        /// 根据连接ID获取对应的连接信息对象。
        /// </summary>
        /// <param name="connectionId">连接的唯一ID。</param>
        /// <returns>匹配的连接信息对象，如果不存在返回 null。</returns>
        OnlineUserInfo? GetConnectionById(string connectionId);

        /// <summary>
        /// 判断指定用户当前是否在线（至少有一个连接）。
        /// </summary>
        /// <param name="userId">用户唯一标识ID。</param>
        /// <returns>如果该用户至少有一个在线连接，则返回 true，否则返回 false。</returns>
        bool IsOnline(string userId);

        /// <summary>
        /// 获取指定用户的所有连接ID列表。
        /// </summary>
        /// <param name="userId">用户唯一标识ID。</param>
        /// <returns>该用户所有在线连接的连接ID集合。</returns>
        List<string> GetConnectionIdsByUserId(string userId);

        /// <summary>
        /// 获取属于指定群组的所有连接信息列表。
        /// 一个连接可以属于多个群组。
        /// </summary>
        /// <param name="groupName">群组名称。</param>
        /// <returns>所有属于该群组的连接信息集合。</returns>
        List<OnlineUserInfo> GetConnectionsByGroup(string groupName);

        /// <summary>
        /// 获取指定租户下所有在线连接信息。
        /// </summary>
        /// <param name="tenantId">租户ID。</param>
        /// <returns>该租户下所有在线连接的连接信息列表。</returns>
        List<OnlineUserInfo> GetConnectionsByTenant(string tenantId);

        /// <summary>
        /// 清空所有连接信息，通常用于调试或服务重启时清理状态。
        /// </summary>
        void ClearAll();

    }
}
