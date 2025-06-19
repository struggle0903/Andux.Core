namespace Andux.Core.RabbitMQ.Models
{
    /// <summary>
    /// 租户RabbitMQ配置
    /// </summary>
    public class TenantOptions
    {
        /// <summary>
        /// 租户ID
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// 虚拟主机(默认租户ID)
        /// </summary>
        public string VirtualHost { get; set; } = "/";

        /// <summary>
        /// 用户名(格式: tenant_{tenantId})
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="tenantId"></param>
        public TenantOptions(string tenantId)
        {
            Password = Password ?? string.Empty;
            TenantId = tenantId ?? string.Empty;
            VirtualHost = VirtualHost;
            UserName = $"{tenantId}";
        }
    }
}
