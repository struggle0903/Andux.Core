using System.Text;

namespace Andux.Core.Extensions
{
    /// <summary>
    /// 异常扩展类
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// 获取异常及其所有内部异常的完整消息
        /// </summary>
        public static string GetFullMessage(this Exception ex)
        {
            var sb = new StringBuilder();
            while (ex != null)
            {
                sb.AppendLine(ex.Message);
                ex = ex.InnerException;
            }
            return sb.ToString().Trim();
        }

        /// <summary>
        /// 获取异常及其所有内部异常的堆栈跟踪信息
        /// </summary>
        public static string GetFullStackTrace(this Exception ex)
        {
            var sb = new StringBuilder();
            while (ex != null)
            {
                sb.AppendLine(ex.StackTrace);
                ex = ex.InnerException;
            }
            return sb.ToString().Trim();
        }

        /// <summary>
        /// 获取异常层次结构中的所有异常类型名称
        /// </summary>
        public static IEnumerable<string> GetAllExceptionTypes(this Exception ex)
        {
            while (ex != null)
            {
                yield return ex.GetType().FullName;
                ex = ex.InnerException;
            }
        }

        /// <summary>
        /// 检查异常是否包含特定类型的内部异常
        /// </summary>
        public static bool ContainsInnerException<T>(this Exception ex) where T : Exception
        {
            while (ex != null)
            {
                if (ex is T)
                    return true;
                ex = ex.InnerException;
            }
            return false;
        }

        /// <summary>
        /// 获取第一个特定类型的内部异常
        /// </summary>
        public static T GetFirstInnerException<T>(this Exception ex) where T : Exception
        {
            while (ex != null)
            {
                if (ex is T typedEx)
                    return typedEx;
                ex = ex.InnerException;
            }
            return null;
        }

        /// <summary>
        /// 获取异常的所有数据键值对
        /// </summary>
        public static Dictionary<string, string> GetExceptionData(this Exception ex)
        {
            var data = new Dictionary<string, string>();
            if (ex?.Data == null)
                return data;

            foreach (var key in ex.Data.Keys)
            {
                data[key.ToString()] = ex.Data[key]?.ToString() ?? string.Empty;
            }
            return data;
        }

        /// <summary>
        /// 将异常转换为格式化的字符串（包含所有内部异常信息）
        /// </summary>
        public static string ToFormattedString(this Exception ex)
        {
            var sb = new StringBuilder();
            int level = 0;

            while (ex != null)
            {
                sb.AppendLine($"[Level {level}] {ex.GetType().FullName}");
                sb.AppendLine($"Message: {ex.Message}");
                sb.AppendLine($"StackTrace: {ex.StackTrace}");

                var data = ex.GetExceptionData();
                if (data.Count > 0)
                {
                    sb.AppendLine("Data:");
                    foreach (var kvp in data)
                    {
                        sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
                    }
                }

                sb.AppendLine(new string('-', 50));
                ex = ex.InnerException;
                level++;
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// 检查异常是否是特定类型或派生自特定类型
        /// </summary>
        public static bool IsOrContains<T>(this Exception ex) where T : Exception
        {
            return ex is T || ex.ContainsInnerException<T>();
        }

        /// <summary>
        /// 获取异常的最底层内部异常（根源异常）
        /// </summary>
        public static Exception GetRootException(this Exception ex)
        {
            if (ex == null)
                return null;

            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            return ex;
        }

        /// <summary>
        /// 获取异常链中所有异常的详细信息（包括类型、消息和堆栈跟踪）
        /// </summary>
        public static List<ExceptionDetail> GetAllExceptionDetails(this Exception ex)
        {
            var details = new List<ExceptionDetail>();
            int level = 0;

            while (ex != null)
            {
                details.Add(new ExceptionDetail
                {
                    Level = level,
                    TypeName = ex.GetType().FullName,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    Data = ex.GetExceptionData()
                });

                ex = ex.InnerException;
                level++;
            }

            return details;
        }

        /// <summary>
        /// 异常详细信息类
        /// </summary>
        public class ExceptionDetail
        {
            public int Level { get; set; }
            public string TypeName { get; set; }
            public string Message { get; set; }
            public string StackTrace { get; set; }
            public Dictionary<string, string> Data { get; set; }
        }

        /// <summary>
        /// 将异常转换为简单的JSON格式字符串
        /// </summary>
        public static string ToSimpleJson(this Exception ex)
        {
            var details = ex.GetAllExceptionDetails();
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"exceptions\": [");

            for (int i = 0; i < details.Count; i++)
            {
                var detail = details[i];
                sb.AppendLine("    {");
                sb.AppendLine($"      \"level\": {detail.Level},");
                sb.AppendLine($"      \"type\": \"{detail.TypeName}\",");
                sb.AppendLine($"      \"message\": \"{EscapeJsonString(detail.Message)}\",");

                if (detail.Data.Count > 0)
                {
                    sb.AppendLine("      \"data\": {");
                    bool first = true;
                    foreach (var kvp in detail.Data)
                    {
                        if (!first) sb.AppendLine(",");
                        sb.Append($"        \"{EscapeJsonString(kvp.Key)}\": \"{EscapeJsonString(kvp.Value)}\"");
                        first = false;
                    }
                    sb.AppendLine();
                    sb.AppendLine("      },");
                }

                sb.AppendLine($"      \"stackTrace\": \"{EscapeJsonString(detail.StackTrace)}\"");
                sb.Append(i < details.Count - 1 ? "    }," : "    }");
            }

            sb.AppendLine("  ]");
            sb.AppendLine("}");
            return sb.ToString();
        }

        /// <summary>
        /// 字符串进行 JSON 转义
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string EscapeJsonString(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input.Replace("\\", "\\\\")
                       .Replace("\"", "\\\"")
                       .Replace("\r", "\\r")
                       .Replace("\n", "\\n")
                       .Replace("\t", "\\t");
        }

        /// <summary>
        /// 将异常信息记录到指定文件
        /// </summary>
        public static void LogToFile(this Exception ex, string filePath, bool append = true)
        {
            var logContent = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]\n" +
                            ex.ToFormattedString() + "\n\n";
            File.AppendAllText(filePath, logContent);
        }

        /// <summary>
        /// 将异常信息输出到控制台（错误消息显示为红色）
        /// </summary>
        public static void LogToConsole(this Exception ex)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.ToFormattedString());
            Console.ForegroundColor = originalColor;
        }

        /// <summary>
        /// 执行方法并在失败时重试指定次数
        /// </summary>
        public static T ExecuteWithRetry<T>(this Func<T> func, int maxRetries = 3, int delayMs = 1000)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    return func();
                }
                catch (Exception ex) when (retryCount < maxRetries)
                {
                    retryCount++;
                    Thread.Sleep(delayMs);
                }
            }
        }

        /// <summary>
        /// 执行方法并在特定异常发生时重试
        /// </summary>
        public static T ExecuteWithConditionalRetry<T>(this Func<T> func,
            Func<Exception, bool> shouldRetry,
            int maxRetries = 3,
            int delayMs = 1000)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    return func();
                }
                catch (Exception ex) when (shouldRetry(ex) && retryCount < maxRetries)
                {
                    retryCount++;
                    Thread.Sleep(delayMs);
                }
            }
        }

        /// <summary>
        /// 检查异常是否是超时异常或包含超时内部异常
        /// </summary>
        public static bool IsTimeoutException(this Exception ex)
        {
            return ex is TimeoutException ||
                   ex.ContainsInnerException<TimeoutException>();
        }

        /// <summary>
        /// 检查异常是否是数据库相关异常
        /// </summary>
        public static bool IsDatabaseException(this Exception ex)
        {
            return ex is System.Data.Common.DbException ||
                   ex.ContainsInnerException<System.Data.Common.DbException>();
        }

        /// <summary>
        /// 获取异常来源方法（调用堆栈中的第一个非系统方法）
        /// </summary>
        public static string GetOriginMethod(this Exception ex)
        {
            if (ex?.StackTrace == null) return null;

            var lines = ex.StackTrace.Split('\n');
            foreach (var line in lines)
            {
                if (!line.Contains("System.") && !line.Contains("Microsoft."))
                {
                    return line.Trim();
                }
            }
            return lines.FirstOrDefault()?.Trim();
        }

        /// <summary>
        /// 忽略特定类型的异常（如果匹配）
        /// </summary>
        public static void IgnoreIf<T>(this Exception ex) where T : Exception
        {
            if (ex is T) return;
            throw ex;
        }

        /// <summary>
        /// 根据条件过滤异常
        /// </summary>
        public static Exception Filter(this Exception ex, Func<Exception, bool> predicate)
        {
            return predicate(ex) ? ex : null;
        }

        /// <summary>
        /// 将异常转换为API错误响应对象
        /// </summary>
        public static ApiErrorResponse ToApiErrorResponse(this Exception ex, bool includeDetails = false)
        {
            return new ApiErrorResponse
            {
                ErrorCode = ex is BusinessException be ? be.Code.ToString() : "SYSTEM_ERROR",
                Message = ex.Message,
                Details = includeDetails ? ex.ToFormattedString() : null,
                Timestamp = DateTime.UtcNow
            };
        }

        public class ApiErrorResponse
        {
            public string ErrorCode { get; set; }
            public string Message { get; set; }
            public string Details { get; set; }
            public DateTime Timestamp { get; set; }
        }

        /// <summary>
        /// 将异常包装为业务异常
        /// </summary>
        public static BusinessException WrapAsBusinessException(this Exception ex, string customMessage = null)
        {
            return new BusinessException(customMessage ?? ex.Message, ex);
        }
    }
}
