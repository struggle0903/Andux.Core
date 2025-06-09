// =======================================
// 作者：andy.hu
// 文件：LoggingOptions.cs
// 描述：日志配置选项类，用于从配置文件读取日志相关设置
// =======================================

namespace Andux.Core.Logger
{
    /// <summary>
    /// 日志配置选项类，用于从配置文件读取日志相关设置
    /// </summary>
    public class LoggingOptions
    {
        /// <summary>
        /// 是否启用控制台日志
        /// </summary>
        public bool EnableConsole { get; set; } = true;

        /// <summary>
        /// 是否启用文件日志
        /// </summary>
        public bool EnableFile { get; set; } = false;

        /// <summary>
        /// 是否启用 Seq 日志输出
        /// </summary>
        public bool EnableSeq { get; set; } = false;

        /// <summary>
        /// 文件日志保存路径，支持 Rolling 日志
        /// </summary>
        public string FilePath { get; set; } = "Logs/log-.txt";

        /// <summary>
        /// Seq 服务器地址 - 默认地址为：http://localhost:5341
        /// </summary>
        public string SeqUrl { get; set; } = "http://localhost:5341";

        /// <summary>
        /// 最低日志级别（如：Debug、Information、Warning、Error）
        /// </summary>
        public string MinimumLevel { get; set; } = "Information";

        /// <summary>
        /// 文件日志保留多少个滚动文件（按天） - 默认7天
        /// </summary>
        public int FileRetainedFileCountLimit { get; set; } = 7;

        /// <summary>
        /// 应用名称
        /// </summary>
        public string AppName { get; set; } = "Andux.Core";

        /// <summary>
        /// 全局日志标签，用于添加额外的上下文信息到日志中
        /// </summary>
        public Dictionary<string, string> GlobalTags { get; set; } = new();

        /// <summary>
        /// 不同命名空间的最小日志等级覆盖
        /// </summary>
        public Dictionary<string, string> OverrideLevels { get; set; } = new();
    }
}
