namespace Andux.Core.RabbitMQ.Models
{
    /// <summary>
    /// RabbitMQ 全局配置
    /// </summary>
    public class RabbitMQOptions
    {
        /// <summary>
        /// 主机地址 (默认: localhost)
        /// </summary>
        public string HostName { get; set; } = "localhost";

        /// <summary>
        /// 端口号 (默认: 5672)
        /// </summary>
        public int Port { get; set; } = 5672;

        /// <summary>
        /// 虚拟主机 (默认: /)
        /// </summary>
        public string VirtualHost { get; set; } = "/";

        /// <summary>
        /// 用户名 (默认: guest)
        /// </summary>
        public string UserName { get; set; } = "guest";

        /// <summary>
        /// 密码 (默认: guest)
        /// </summary>
        public string Password { get; set; } = "guest";

        /// <summary>
        /// 客户提供的名称 (默认: Andux.Core.RabbitMQ)
        /// </summary>
        public string ClientProvidedName { get; set; } = "Andux.Core.RabbitMQ";

        /// <summary>
        /// 启用自动恢复 (默认: true)
        /// </summary>
        public bool AutomaticRecoveryEnabled { get; set; } = true;

        /// <summary>
        /// 网络恢复间隔(秒) (默认: 10)
        /// </summary>
        public int NetworkRecoveryInterval { get; set; } = 10;
    }
}
