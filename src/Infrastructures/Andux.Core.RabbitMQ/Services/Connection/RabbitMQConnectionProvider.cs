using Andux.Core.RabbitMQ.Exceptions;
using Andux.Core.RabbitMQ.Interfaces;
using Andux.Core.RabbitMQ.Models;
using RabbitMQ.Client;
using System.Collections.Concurrent;

namespace Andux.Core.RabbitMQ.Services.Connection
{
    /// <summary>
    /// RabbitMQ连接提供者(支持多租户)
    /// </summary>
    public class RabbitMQConnectionProvider : IRabbitMQConnectionProvider
    {
        private readonly RabbitMQOptions _globalOptions;
        private readonly ConcurrentDictionary<string, TenantOptions> _tenantOptions;
        private readonly ConcurrentDictionary<string, IConnection> _connections;
        private bool _disposed;
        
        /// <summary>
        /// 连接对象锁
        /// </summary>
        private readonly object _connectionLock = new object();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="globalOptions"></param>
        public RabbitMQConnectionProvider(RabbitMQOptions globalOptions)
        {
            _globalOptions = globalOptions;
            _tenantOptions = new ConcurrentDictionary<string, TenantOptions>();
            _connections = new ConcurrentDictionary<string, IConnection>();
        }

        /// <summary>
        /// 注册租户配置
        /// </summary>
        /// <param name="options"></param>
        public void RegisterTenant(TenantOptions options)
        {
            _tenantOptions[options.TenantId] = options;
        }

        /// <summary>
        /// 获取默认连接
        /// </summary>
        /// <returns></returns>
        public IConnection GetConnection()
        {
            return GetOrCreateConnection(null);
        }

        /// <summary>
        /// 获取租户专属连接
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public IConnection GetTenantConnection(string? tenantId)
        {
            //if (string.IsNullOrEmpty(tenantId))
            //    throw new ArgumentException("Tenant ID cannot be null or empty");

            return GetOrCreateConnection(tenantId);
        }

        /// <summary>
        /// 创建通道
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public IModel CreateChannel(string? tenantId = null)
        {
            var connection = string.IsNullOrEmpty(tenantId)
                ? GetConnection()
                : GetTenantConnection(tenantId);

            return connection.CreateModel();
        }

        /// <summary>
        /// 删除指定连接
        /// </summary>
        /// <param name="tenantId"></param>
        public void RemoveConnection(string tenantId)
        {
            lock (_connectionLock)
            {
                if (_connections.TryGetValue(tenantId, out var conn))
                {
                    conn.Dispose();
                    _connections.TryRemove(tenantId, out _);
                }
            }
        }

        /// <summary>
        /// 获取或创建连接
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        /// <exception cref="RabbitMQException"></exception>
        private IConnection GetOrCreateConnection(string? tenantId)
        {
            var key = tenantId ?? "default";

            // 第一重检查（快速路径）
            if (_connections.TryGetValue(key, out var existingConn) && existingConn?.IsOpen == true)
            {
                return existingConn;
            }

            // 加锁确保线程安全
            lock (_connectionLock)
            {
                // 第二重检查（防止重复创建）
                if (_connections.TryGetValue(key, out existingConn) && existingConn?.IsOpen == true)
                {
                    return existingConn;
                }

                // 创建新连接
                var newConnection = CreateConnectionFactory(tenantId).CreateConnection();
                newConnection.ConnectionShutdown += (sender, args) => RemoveConnection(key);

                // 原子性替换旧连接
                if (_connections.TryGetValue(key, out var oldConn))
                {
                    oldConn?.Dispose();
                    _connections[key] = newConnection; // 直接更新字典
                }
                else
                {
                    if (!_connections.TryAdd(key, newConnection))
                    {
                        newConnection.Dispose();
                        throw new RabbitMQException($"连接字典竞争失败（租户: {key}）");
                    }
                }

                return newConnection;
            }
        }

        /// <summary>
        /// 获取当前所有的连接对象
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, IConnection> GetAllConnections()
        {
            return _connections;
        }

        private ConnectionFactory CreateConnectionFactory(string? tenantId)
        {
            var options = string.IsNullOrEmpty(tenantId)
                ? _globalOptions
                : GetTenantOptions(tenantId);

            return new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                VirtualHost = options.VirtualHost,
                UserName = options.UserName,
                Password = options.Password,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(options.NetworkRecoveryInterval)
            };
        }

        private RabbitMQOptions GetTenantOptions(string tenantId)
        {
            if (!_tenantOptions.TryGetValue(tenantId, out var tenantOptions))
                throw new RabbitMQException($"Tenant {tenantId} not registered");

            return new RabbitMQOptions
            {
                HostName = _globalOptions.HostName,
                Port = _globalOptions.Port,
                VirtualHost = tenantOptions.VirtualHost,
                UserName = tenantOptions.UserName,
                Password = tenantOptions.Password
            };
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            foreach (var conn in _connections.Values)
                conn.Dispose();

            _connections.Clear();
        }

    }
}
