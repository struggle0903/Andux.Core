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
        /// SignalR 客户端构造函数。
        /// </summary>
        /// <param name="url"></param>
        public SignalRClient(string url)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();
        }

        public async Task ConnectAsync()
        {
            await _connection.StartAsync();
        }

        public async Task SendMessageAsync(string method, params object[] args)
        {
            await _connection.InvokeCoreAsync(method, typeof(object), args);
        }

        public async Task SendMessageAsync(string method, object arg1, object arg2)
        {
            await _connection.InvokeAsync(method, arg1, arg2);
        }

        public void On<T1>(string method, Action<T1> handler)
        {
            _connection.On(method, handler);
        }

        public void On<T1, T2>(string method, Action<T1, T2> handler)
        {
            _connection.On(method, handler);
        }
        public void On(string method, Action handler)
        {
            _connection.On(method, handler);
        }

        public async Task DisconnectAsync()
        {
            await _connection.StopAsync();
        }
    }
}
