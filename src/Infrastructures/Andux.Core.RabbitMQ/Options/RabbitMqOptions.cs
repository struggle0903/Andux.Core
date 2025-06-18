namespace Andux.Core.RabbitMQ.Options
{
    /// <summary>
    /// 多用户 RabbitMQ 配置集合。
    /// </summary>
    public class RabbitMqOptions
    {
        /// <summary>
        /// 多用户配置
        /// </summary>
        public List<RabbitMqUserOptions> Users { get; set; } = new();
    }
}
