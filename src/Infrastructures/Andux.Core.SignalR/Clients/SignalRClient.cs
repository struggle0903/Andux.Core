using Microsoft.AspNetCore.SignalR.Client;

namespace Andux.Core.SignalR.Clients
{
    /// <summary>
    /// SignalR C# 客户端封装。
    /// </summary>
    public class SignalRClient
    {
        private readonly HubConnection _connection;

        /// <summary>
        /// SignalR 客户端构造函数
        /// </summary>
        /// <param name="url">Hub 地址</param>
        /// <param name="token">JWT Token，可选</param>
        public SignalRClient(string url, string? token = null)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(url, options =>
                {
                    // 如果要使用租户模式，token为必须，否则拿不到tenantId
                    if (!string.IsNullOrEmpty(token))
                    {
                        options.AccessTokenProvider = () => Task.FromResult(token);
                    }
                })
                .WithAutomaticReconnect()
                .Build();
        }

        /// <summary>
        /// 客户端连接
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            await _connection.StartAsync();
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task SendMessageAsync(string method, params object[] args)
        {
            await _connection.InvokeCoreAsync(method, typeof(object), args);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="method"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        public async Task SendMessageAsync(string method, object arg1, object arg2)
        {
            await _connection.InvokeAsync(method, arg1, arg2);
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="method"></param>
        /// <param name="handler"></param>
        public void On<T1>(string method, Action<T1> handler)
        {
            _connection.On(method, handler);
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="method"></param>
        /// <param name="handler"></param>
        public void On<T1, T2>(string method, Action<T1, T2> handler)
        {
            _connection.On(method, handler);
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <param name="method"></param>
        /// <param name="handler"></param>
        public void On(string method, Action handler)
        {
            _connection.On(method, handler);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            await _connection.StopAsync();
        }
    }
}
