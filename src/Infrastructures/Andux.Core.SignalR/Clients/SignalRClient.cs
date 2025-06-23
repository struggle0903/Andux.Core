using Microsoft.AspNetCore.SignalR.Client;

namespace Andux.Core.SignalR.Clients
{
    /// <summary>
    /// SignalR C# 客户端封装。
    /// </summary>
    public class SignalRClient
    {
        private readonly HubConnection _connection;

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
            await _connection.InvokeAsync(method, args);
        }

        public void On<T1, T2>(string method, Action<T1, T2> handler)
        {
            _connection.On(method, handler);
        }

        public async Task DisconnectAsync()
        {
            await _connection.StopAsync();
        }
    }
}
