﻿using Andux.Core.SignalR.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace Andux.Core.SignalR.Services
{
    /// <summary>
    /// Redis用户连接管理
    /// </summary>
    public class RedisUserConnectionManager : IRedisUserConnectionManager
    {
        private readonly IDatabase _redis;
        private const string RedisKey = "Signalr:Online_Users";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionMultiplexer"></param>
        public RedisUserConnectionManager(IConnectionMultiplexer connectionMultiplexer)
        {
            _redis = connectionMultiplexer.GetDatabase();

            // 程序启动时清理历史连接
            ClearAll();
        }

        /// <summary>
        /// 注册一个新的连接信息，添加到在线连接管理中。
        /// 通常在用户连接建立时调用，记录连接ID、用户ID、所属组、租户等信息。
        /// </summary>
        /// <param name="userInfo">包含连接相关信息的用户连接对象。</param>
        public void AddConnection(OnlineUserInfo userInfo)
        {
            var json = JsonSerializer.Serialize(userInfo);
            _redis.HashSet(RedisKey, userInfo.ConnectionId, json);
        }

        /// <summary>
        /// 移除指定连接ID的连接信息，通常在连接断开时调用。
        /// </summary>
        /// <param name="connectionId">要移除的连接ID。</param>
        public void RemoveConnection(string connectionId)
        {
            _redis.HashDelete(RedisKey, connectionId);
        }

        /// <summary>
        /// 获取当前所有在线的连接列表，包含所有用户的所有连接。
        /// </summary>
        /// <returns>返回所有在线连接信息的列表。</returns>
        public List<OnlineUserInfo> GetAllConnections()
        {
            var entries = _redis.HashGetAll(RedisKey);
            return entries
                .Select(e => JsonSerializer.Deserialize<OnlineUserInfo>(e.Value!)!)
                .ToList();
        }

        /// <summary>
        /// 根据用户ID获取该用户所有的连接信息列表。
        /// 一个用户可能有多个连接（比如多端登录）。
        /// </summary>
        /// <param name="userId">用户的唯一标识ID。</param>
        /// <returns>该用户所有连接信息列表。</returns>
        public List<OnlineUserInfo> GetConnectionsByUserId(string userId)
        {
            return GetAllConnections()
                .Where(x => x.UserId == userId)
                .ToList();
        }

        /// <summary>
        /// 根据连接ID获取对应的连接信息对象。
        /// </summary>
        /// <param name="connectionId">连接的唯一ID。</param>
        /// <returns>匹配的连接信息对象，如果不存在返回 null。</returns>
        public OnlineUserInfo? GetConnectionById(string connectionId)
        {
            var val = _redis.HashGet(RedisKey, connectionId);
            return val.HasValue ? JsonSerializer.Deserialize<OnlineUserInfo>(val!) : null;
        }

        /// <summary>
        /// 判断指定用户当前是否在线（至少有一个连接）。
        /// </summary>
        /// <param name="userId">用户唯一标识ID。</param>
        /// <returns>如果该用户至少有一个在线连接，则返回 true，否则返回 false。</returns>
        public bool IsOnline(string userId)
        {
            return GetAllConnections().Any(x => x.UserId == userId);
        }

        /// <summary>
        /// 获取指定用户的所有连接ID列表。
        /// </summary>
        /// <param name="userId">用户唯一标识ID。</param>
        /// <returns>该用户所有在线连接的连接ID集合。</returns>
        public List<string> GetConnectionIdsByUserId(string userId)
        {
            return GetAllConnections()
                .Where(x => x.UserId == userId)
                .Select(x => x.ConnectionId)
                .ToList();
        }

        /// <summary>
        /// 获取属于指定群组的所有连接信息列表。
        /// 一个连接可以属于多个群组。
        /// </summary>
        /// <param name="groupName">群组名称。</param>
        /// <returns>所有属于该群组的连接信息集合。</returns>
        public List<OnlineUserInfo> GetConnectionsByGroup(string groupName)
        {
            return GetAllConnections()
                .Where(x => x.Groups.Contains(groupName))
                .ToList();
        }

        /// <summary>
        /// 获取指定租户下所有在线连接信息。
        /// </summary>
        /// <param name="tenantId">租户ID。</param>
        /// <returns>该租户下所有在线连接的连接信息列表。</returns>
        public List<OnlineUserInfo> GetConnectionsByTenant(string tenantId)
        {
            return GetAllConnections()
                .Where(x => x.TenantId == tenantId)
                .ToList();
        }

        /// <summary>
        /// 清空所有连接信息，通常用于调试或服务重启时清理状态。
        /// </summary>
        public void ClearAll()
        {
            _redis.KeyDelete(RedisKey);
        }

    }
}
