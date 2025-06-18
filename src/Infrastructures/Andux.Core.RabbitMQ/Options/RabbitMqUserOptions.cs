namespace Andux.Core.RabbitMQ.Options
{
    /// <summary>
    /// 单个 RabbitMQ 用户配置。
    /// </summary>
    public class RabbitMqUserOptions
    {
        public string UserName { get; set; } = "guest";     // 用户名
        public string Password { get; set; } = "guest";     // 密码
        public string HostName { get; set; } = "localhost"; // 主机名
        public int Port { get; set; } = 5672;               // 端口
        public string VirtualHost { get; set; } = "/";       // 虚拟主机
        public string ClientProvidedName { get; set; } = "AppClient"; // 客户端提供的名称
    }

    /*
     * {
          "RabbitMQ": {
            "Users": [
              {
                "UserName": "User1",
                "HostName": "localhost",
                "Password": "password1",
                "VirtualHost": "/",
                "Port": 5672,
                "ClientProvidedName": "AppClient"
              },
              {
                "UserName": "User2",
                "HostName": "localhost",
                "Password": "password2",
                "VirtualHost": "/",
                "Port": 5672,
                "ClientProvidedName": "AppClient2"
              }
            ]
          }
        }
     */
}
