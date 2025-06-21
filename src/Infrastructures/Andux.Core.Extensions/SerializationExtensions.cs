using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Andux.Core.Extensions
{
    /// <summary>
    /// 序列号/反序列化扩展类
    /// </summary>
    public static class SerializationExtensions
    {
        #region JSON 序列化
        // 默认配置（避免每次调用都新建 Options）
        private static readonly JsonSerializerOptions DefaultOptions = new()
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 支持中文
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // 忽略 null 值
        };

        /// <summary>
        /// 将对象序列化为 JSON 字符串（使用默认配置）
        /// </summary>
        public static string ToJson<T>(this T obj, bool indented = false)
        {
            return ToJson(obj, options =>
            {
                options.WriteIndented = indented;
            });
        }

        /// <summary>
        /// 将对象序列化为 JSON 字符串（支持自定义配置）
        /// var json = obj.ToJson(opts =>
        /// {
        ///     opts.WriteIndented = true;
        ///     opts.PropertyNamingPolicy = null;
        /// });
        /// </summary>
        public static string ToJson<T>(this T obj, Action<JsonSerializerOptions>? configure = null)
        {
            if (obj is null)
                return "null"; // 或者 throw new ArgumentNullException(nameof(obj));

            try
            {
                // 克隆默认配置（避免修改全局配置）
                var options = new JsonSerializerOptions(DefaultOptions);
                configure?.Invoke(options); // 允许外部自定义配置

                return JsonSerializer.Serialize(obj, options);
            }
            catch (JsonException ex)
            {
                // 可记录日志或抛出自定义异常
                throw new InvalidOperationException("JSON 序列化失败", ex);
            }
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为对象
        /// </summary>
        public static T? FromJson<T>(this string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        #endregion

        #region XML 序列化
        /// <summary>
        /// 将对象序列化为 XML 字符串
        /// </summary>
        public static string ToXml<T>(this T obj)
        {
            var serializer = new XmlSerializer(typeof(T));
            using var writer = new StringWriter();
            serializer.Serialize(writer, obj);
            return writer.ToString();
        }

        /// <summary>
        /// 将 XML 字符串反序列化为对象
        /// </summary>
        public static T? FromXml<T>(this string xml)
        {
            var serializer = new XmlSerializer(typeof(T));
            using var reader = new StringReader(xml);
            return (T?)serializer.Deserialize(reader);
        }

        /// <summary>
        /// 将 XML 字符串转换为 JSON 字符串
        /// </summary>
        public static string XmlToJson(this string xml, bool indented = false)
        {
            if (string.IsNullOrWhiteSpace(xml))
                throw new ArgumentException("XML 不能为空", nameof(xml));

            try
            {
                // 1. 解析 XML
                var xmlDoc = XDocument.Parse(xml);

                // 2. 转换为 JSON
                var options = new JsonSerializerOptions
                {
                    WriteIndented = indented,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 避免中文被转义
                };

                return JsonSerializer.Serialize(xmlDoc, options);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("XML 转 JSON 失败", ex);
            }
        }

        /// <summary>
        /// 将 XML 文件读取并转换为 JSON 字符串
        /// </summary>
        public static string XmlFileToJson(this string filePath, bool indented = false)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("XML 文件不存在", filePath);

            string xml = File.ReadAllText(filePath);
            return xml.XmlToJson(indented);
        }
        #endregion

    }
}
