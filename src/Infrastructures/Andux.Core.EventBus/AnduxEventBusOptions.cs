namespace Andux.Core.EventBus
{
    /// <summary>
    /// EventBus 通用配置选项（支持 InMemory、RabbitMQ 等）
    /// </summary>
    public class AnduxEventBusOptions
    {
        /// <summary>
        /// 消息中间件提供程序，支持的值例如："InMemory"、"RabbitMQ"
        /// </summary>
        public string Provider { get; set; } = "InMemory";

        /// <summary>
        /// 消息中间件的主机名（仅当使用 RabbitMQ 等网络服务时需要）
        /// </summary>
        public string? HostName { get; set; }

        /// <summary>
        /// 连接端口，RabbitMQ 默认是 5672
        /// </summary>
        public int? Port { get; set; } = 5672;

        /// <summary>
        /// 连接用户名（RabbitMQ 需要认证）
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// 连接密码（RabbitMQ 需要认证）
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// 虚拟主机（RabbitMQ 支持多虚拟主机），默认为 "/"（根）
        /// </summary>
        public string? VirtualHost { get; set; }
    }
}
