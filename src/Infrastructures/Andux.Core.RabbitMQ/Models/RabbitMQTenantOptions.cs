namespace Andux.Core.RabbitMQ.Models
{
    /// <summary>
    /// 租户RabbitMQ配置
    /// </summary>
    public class RabbitMQTenantOptions
    {
        /// <summary>
        /// 租户ID
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// 主机地址 (默认: localhost)
        /// </summary>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// 端口号 (默认: 5672)
        /// </summary>
        public int Port { get; set; } = 5672;

        /// <summary>
        /// 虚拟主机(默认租户ID)
        /// </summary>
        public string VirtualHost { get; set; } = "/";

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        #region 为兼容OMP之前项目的配置项保留
        /// <summary>
        /// 是否启用交换机前缀，启用后exchange前面会带上 TenantId
        /// </summary>
        public bool EnableExchangePrefix { get; set; } = true;

        /// <summary>
        /// 是否启用路由key（routingKey）前缀，启用后routingKey前面会带上 TenantId
        /// </summary>
        public bool EnableRoutingKeyPrefix { get; set; } = true;

        /// <summary>
        /// 是否启用队列名前缀，启用后queueName前面会带上 TenantId
        /// </summary>
        public bool EnableQueueNamePrefix { get; set; } = true;
        #endregion

        /// <summary>
        /// 网络恢复间隔(秒) (默认: 10)
        /// </summary>
        public int NetworkRecoveryInterval { get; set; } = 10;
    }
}
