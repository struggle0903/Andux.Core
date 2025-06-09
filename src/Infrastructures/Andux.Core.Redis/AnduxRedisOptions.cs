// =======================================
// 作者：andy.hu
// 文件：AnduxRedisOptions.cs
// 描述：Redis配置选项类，用于初始化Redis
// =======================================

namespace Andux.Core.Redis
{
    /// <summary>
    /// Redis 配置项，供 DI 绑定 appsettings.json 使用
    /// </summary>
    public class AnduxRedisOptions
    {
        /// <summary>
        /// Redis 连接字符串
        /// </summary>
        public string Configuration { get; set; } = string.Empty;

        /// <summary>
        /// 默认数据库索引
        /// </summary>
        public int DefaultDatabase { get; set; } = 0;

        /// <summary>
        /// 实例前缀（用于 key 前缀）
        /// </summary>
        public string? InstanceName { get; set; }
    }
}
