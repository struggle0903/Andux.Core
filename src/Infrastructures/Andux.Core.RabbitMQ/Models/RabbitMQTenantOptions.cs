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

        /// <summary>
        /// 网络恢复间隔(秒) (默认: 10)
        /// </summary>
        public int NetworkRecoveryInterval { get; set; } = 10;

        ///// <summary>
        ///// 构造
        ///// </summary>
        ///// <param name="tenantId"></param>
        //public RabbitMQTenantOptions(string tenantId)
        //{
        //    Password = Password ?? string.Empty;
        //    TenantId = tenantId ?? string.Empty;
        //    VirtualHost = VirtualHost;
        //    UserName = $"{tenantId}";
        //}
    }
}
