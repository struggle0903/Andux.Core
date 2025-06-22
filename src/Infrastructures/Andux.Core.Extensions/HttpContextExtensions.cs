using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Andux.Core.Extensions
{
    /// <summary>
    /// HttpContext 扩展
    /// </summary>
    public static class HttpContextExtensions
    {
        #region 请求信息获取

        /// <summary>
        /// 获取客户端IP地址（考虑代理情况）
        /// </summary>
        public static string GetClientIp(this HttpContext context)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Connection.RemoteIpAddress?.ToString();
            }
            return ip;
        }

        /// <summary>
        /// 获取请求的完整URL
        /// </summary>
        public static string GetFullUrl(this HttpContext context)
        {
            var request = context.Request;
            return $"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}{request.QueryString}";
        }

        /// <summary>
        /// 获取请求的基URL（协议+主机）
        /// </summary>
        public static string GetBaseUrl(this HttpContext context)
        {
            return $"{context.Request.Scheme}://{context.Request.Host}";
        }

        /// <summary>
        /// 获取请求的User-Agent
        /// </summary>
        public static string GetUserAgent(this HttpContext context)
        {
            return context.Request.Headers["User-Agent"].ToString();
        }

        /// <summary>
        /// 获取请求头值（不区分大小写）
        /// </summary>
        public static string GetHeader(this HttpContext context, string headerName)
        {
            return context.Request.Headers[headerName].ToString();
        }

        #endregion

        #region 用户认证与授权

        /// <summary>
        /// 获取当前用户ID（从Claim中）
        /// </summary>
        public static string GetUserId(this HttpContext context)
        {
            return context.User?.Claims?
              .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?
              .Value ?? string.Empty;
        }

        /// <summary>
        /// 获取当前用户名（从Claim中）
        /// </summary>
        public static string GetUserName(this HttpContext context)
        {
            return context.User?.Claims?
               .FirstOrDefault(c => c.Type == ClaimTypes.Name)?
               .Value ?? string.Empty;
        }

        /// <summary>
        /// 检查当前用户是否拥有指定角色
        /// </summary>
        public static bool IsInRole(this HttpContext context, string role)
        {
            return context.User?.IsInRole(role) ?? false;
        }

        /// <summary>
        /// 检查当前用户是否有指定权限（基于Claim）
        /// </summary>
        public static bool HasPermission(this HttpContext context, string permission)
        {
            return context.User?.HasClaim(c => c.Type == "permission" && c.Value == permission) ?? false;
        }

        #endregion

        #region 请求数据处理

        /// <summary>
        /// 获取查询字符串参数值
        /// </summary>
        public static string GetQueryParam(this HttpContext context, string key)
        {
            return context.Request.Query[key].ToString();
        }

        /// <summary>
        /// 获取表单数据值
        /// </summary>
        public static string GetFormValue(this HttpContext context, string key)
        {
            return context.Request.Form[key].ToString();
        }

        /// <summary>
        /// 从请求体读取JSON并反序列化为对象
        /// </summary>
        public static async Task<T> ReadBodyAsJsonAsync<T>(this HttpContext context)
        {
            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            return JsonConvert.DeserializeObject<T>(body);
        }

        /// <summary>
        /// 获取所有请求头（字典形式）
        /// </summary>
        public static Dictionary<string, string> GetAllHeaders(this HttpContext context)
        {
            return context.Request.Headers.ToDictionary(
                h => h.Key,
                h => h.Value.ToString());
        }

        #endregion

        #region 响应操作

        /// <summary>
        /// 设置响应状态码并返回JSON
        /// </summary>
        public static async Task WriteJsonResponseAsync(
            this HttpContext context,
            object data,
            int statusCode = 200)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(data));
        }

        /// <summary>
        /// 设置响应缓存头
        /// </summary>
        public static void SetCacheControl(
            this HttpContext context,
            int maxAgeSeconds,
            bool mustRevalidate = true)
        {
            context.Response.Headers["Cache-Control"] =
                $"public, max-age={maxAgeSeconds}{(mustRevalidate ? ", must-revalidate" : "")}";
        }

        /// <summary>
        /// 设置响应下载文件头
        /// </summary>
        public static void SetFileDownloadHeader(
            this HttpContext context,
            string fileName)
        {
            var contentDisposition = new StringValues($"attachment; filename=\"{fileName}\"");
            context.Response.Headers.Add("Content-Disposition", contentDisposition);
        }

        #endregion

        #region 请求上下文操作

        /// <summary>
        /// 获取或设置请求上下文项（线程安全）
        /// </summary>
        public static T GetItem<T>(this HttpContext context, string key, Func<T> factory = null)
        {
            if (context.Items.TryGetValue(key, out var value))
            {
                return (T)value;
            }

            if (factory != null)
            {
                value = factory();
                context.Items[key] = value;
                return (T)value;
            }

            return default;
        }

        /// <summary>
        /// 设置请求上下文项
        /// </summary>
        public static void SetItem<T>(this HttpContext context, string key, T value)
        {
            context.Items[key] = value;
        }

        #endregion

        #region 请求验证

        /// <summary>
        /// 检查请求是否来自AJAX调用
        /// </summary>
        public static bool IsAjaxRequest(this HttpContext context)
        {
            return context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        /// <summary>
        /// 检查请求内容类型是否为JSON
        /// </summary>
        public static bool IsJsonRequest(this HttpContext context)
        {
            return context.Request.ContentType?.StartsWith("application/json") ?? false;
        }

        /// <summary>
        /// 检查请求方法是否为指定类型
        /// </summary>
        public static bool IsHttpMethod(this HttpContext context, string method)
        {
            return context.Request.Method.Equals(method, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Cookie操作

        /// <summary>
        /// 设置Cookie
        /// </summary>
        public static void SetCookie(
            this HttpContext context,
            string key,
            string value,
            int? expireSeconds = null,
            bool httpOnly = true)
        {
            var options = new CookieOptions
            {
                HttpOnly = httpOnly,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax
            };

            if (expireSeconds.HasValue)
            {
                options.Expires = DateTimeOffset.Now.AddSeconds(expireSeconds.Value);
            }

            context.Response.Cookies.Append(key, value, options);
        }

        /// <summary>
        /// 获取Cookie值
        /// </summary>
        public static string GetCookie(this HttpContext context, string key)
        {
            return context.Request.Cookies[key];
        }

        /// <summary>
        /// 删除Cookie
        /// </summary>
        public static void DeleteCookie(this HttpContext context, string key)
        {
            context.Response.Cookies.Delete(key);
        }

        #endregion

        #region 性能诊断

        /// <summary>
        /// 获取请求处理时间（毫秒）
        /// </summary>
        public static long GetRequestDuration(this HttpContext context)
        {
            if (context.Items.TryGetValue("RequestStartTime", out var startTimeObj) &&
                startTimeObj is DateTime startTime)
            {
                return (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
            }
            return -1;
        }

        /// <summary>
        /// 记录请求开始时间（用于性能跟踪）
        /// </summary>
        public static void RecordRequestStartTime(this HttpContext context)
        {
            context.Items["RequestStartTime"] = DateTime.UtcNow;
        }

        #endregion
    }
}
