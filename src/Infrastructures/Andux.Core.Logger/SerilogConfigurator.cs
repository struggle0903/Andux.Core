// =======================================
// 作者：andy.hu
// 文件：SerilogConfigurator.cs
// 描述：Serilog 日志初始化配置类
// =======================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace Andux.Core.Logger
{
    /// <summary>
    /// Serilog 日志初始化配置类
    /// </summary>
    public static class SerilogConfigurator
    {
        /// <summary>
        /// 扩展方法：向服务中注入 Serilog 日志配置
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置对象</param>
        public static void AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
        {
            // 从配置中读取 LoggingOptions 节点
            var options = configuration.GetSection("LoggingOptions").Get<LoggingOptions>() ?? new LoggingOptions();

            // 创建 Serilog 配置对象，设置最小日志级别和上下文信息
            var loggerConfig = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("AppName", options.AppName)
                .MinimumLevel.Is(ParseLogLevel(options.MinimumLevel));

            // 添加不同命名空间的日志级别覆盖（OverrideLevels）
            if (options.OverrideLevels.Any())
            {
                foreach (var kvp in options.OverrideLevels)
                {
                    if (Enum.TryParse<LogEventLevel>(kvp.Value, true, out var overrideLevel))
                    {
                        loggerConfig.MinimumLevel.Override(kvp.Key, overrideLevel);
                    }
                }
            }

            // 添加全局标签
            if (options.GlobalTags?.Any() == true)
            {
                foreach (var tag in options.GlobalTags)
                    loggerConfig.Enrich.WithProperty(tag.Key, tag.Value);
            }

            // 控制台日志
            if (options.EnableConsole)
            {
                loggerConfig.WriteTo.Console();
            }

            // 文件日志，按天滚动保存
            if (options.EnableFile)
            {
                loggerConfig.WriteTo.File(
                    options.FilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: options.FileRetainedFileCountLimit, // 日志文件保留天数
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
                );
            }

            // Seq 日志
            if (options.EnableSeq)
            {
                loggerConfig.WriteTo.Seq(options.SeqUrl);
            }

            // 设置全局日志对象
            Log.Logger = loggerConfig.CreateLogger();

            // 将 Serilog 注册到 .NET 日志系统中
            services.AddLogging(builder =>
            {
                builder.ClearProviders(); // 清除默认日志提供者（ConsoleLoggerProvider）
                builder.AddSerilog();     // 使用配置的 Serilog
            });
        }

        /// <summary>
        /// 将字符串类型的日志级别转换为 Serilog 的 LogEventLevel 枚举类型
        /// </summary>
        /// <param name="level">日志级别字符串</param>
        /// <returns>LogEventLevel 枚举值</returns>
        private static LogEventLevel ParseLogLevel(string level)
        {
            return Enum.TryParse<LogEventLevel>(level, true, out var parsedLevel)
                ? parsedLevel
                : LogEventLevel.Information;
        }

    }
}
