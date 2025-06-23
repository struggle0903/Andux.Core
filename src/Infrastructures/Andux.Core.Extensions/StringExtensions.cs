using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Andux.Core.Extensions
{
    /// <summary>
    /// 提供字符串的扩展方法。
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// 判断字符串是否为 null 或空字符串。
        /// </summary>
        /// <param name="value">要检查的字符串。</param>
        /// <returns>true 表示是 null 或空字符串；否则 false。</returns>
        public static bool IsNullOrEmptyEx(this string? value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// 判断字符串是否为 null 或空白字符串。
        /// </summary>
        /// <param name="value">要检查的字符串。</param>
        /// <returns>true 表示是 null、空字符串或仅包含空白；否则 false。</returns>
        public static bool IsNullOrWhiteSpaceEx(this string? value)
        {
            return string.IsNullOrWhiteSpace(value);
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
        /// 安全截取字符串，超出长度时自动截断。
        /// </summary>
        /// <param name="value">要截取的字符串。</param>
        /// <param name="length">最大长度。</param>
        /// <returns>截取后的字符串。</returns>
        public static string? SafeSubstring(this string? value, int length)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= length ? value : value.Substring(0, length);
        }

        /// <summary>
        /// 将字符串转换为 Base64 编码。
        /// </summary>
        /// <param name="input">输入字符串。</param>
        /// <returns>Base64 编码字符串。</returns>
        public static string ToBase64(this string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 从 Base64 编码字符串解码为原始字符串。
        /// </summary>
        /// <param name="base64">Base64 编码字符串。</param>
        /// <returns>解码后的字符串。</returns>
        public static string FromBase64(this string base64)
        {
            var bytes = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// 将字符串反转。
        /// </summary>
        /// <param name="input">要反转的输入字符串。</param>
        /// <returns>反转后的字符串。</returns>
        public static string Reverse(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var arr = input.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        /// <summary>
        /// 首字母大写。
        /// </summary>
        /// <param name="input">要处理的输入字符串。</param>
        /// <returns>首字母大写的字符串。</returns>
        public static string Capitalize(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            return char.ToUpper(input[0]) + input.Substring(1);
        }

        /// <summary>
        /// 将字符串中的驼峰命名转换为下划线命名（snake_case）。
        /// </summary>
        /// <param name="input">输入字符串。</param>
        /// <returns>转换后的下划线命名字符串。</returns>
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            var result = Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2");
            return result.ToLowerInvariant();
        }

        /// <summary>
        /// 将字符串中的下划线命名转换为驼峰命名（CamelCase）。
        /// </summary>
        /// <param name="input">输入字符串。</param>
        /// <returns>转换后的驼峰命名字符串。</returns>
        public static string ToCamelCase(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            var parts = input.Split(new[] { '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            foreach (var part in parts)
            {
                if (part.Length == 0) continue;
                sb.Append(char.ToUpper(part[0]));
                if (part.Length > 1)
                    sb.Append(part.Substring(1).ToLowerInvariant());
            }
            return sb.ToString();
        }

        /// <summary>
        /// 判断字符串是否包含子串（支持比较选项，例如忽略大小写）。
        /// </summary>
        public static bool ContainsEx(this string source, string value, StringComparison comparison)
        {
            if (source == null || value == null) return false;
            return source.IndexOf(value, comparison) >= 0;
        }

        /// <summary>
        /// 判断字符串是否等于指定值（支持比较选项）。
        /// </summary>
        public static bool EqualsEx(this string source, string value, StringComparison comparison)
        {
            return string.Equals(source, value, comparison);
        }

        /// <summary>
        /// 判断字符串是否匹配正则表达式。
        /// </summary>
        /// <param name="input">要检查的输入字符串。</param>
        /// <param name="pattern">正则表达式模式。</param>
        /// <returns>如果匹配则返回 true，否则返回 false。</returns>
        public static bool IsMatch(this string input, string pattern)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return Regex.IsMatch(input, pattern);
        }

        /// <summary>
        /// 从字符串中提取第一个匹配正则的值。
        /// </summary>
        /// <param name="input">要搜索的输入字符串。</param>
        /// <param name="pattern">正则表达式模式。</param>
        /// <returns>第一个匹配的值，如果没有匹配则返回 null。</returns>
        public static string? ExtractMatch(this string input, string pattern)
        {
            if (string.IsNullOrEmpty(input)) return null;
            var match = Regex.Match(input, pattern);
            return match.Success ? match.Value : null;
        }

        /// <summary>
        /// 安全将字符串转换为 int，如果转换失败返回默认值。
        /// </summary>
        /// <param name="input">要转换的字符串。</param>
        /// <param name="defaultValue">转换失败时返回的默认值（默认为 0）。</param>
        /// <returns>转换后的整数值或默认值。</returns>
        public static int ToInt(this string input, int defaultValue = 0)
            => int.TryParse(input, out var result) ? result : defaultValue;

        /// <summary>
        /// 安全将字符串转换为 long，如果转换失败返回默认值。
        /// </summary>
        /// <param name="input">要转换的字符串。</param>
        /// <param name="defaultValue">转换失败时返回的默认值（默认为 0）。</param>
        /// <returns>转换后的长整数值或默认值。</returns>
        public static long ToLong(this string input, long defaultValue = 0)
            => long.TryParse(input, out var result) ? result : defaultValue;

        /// <summary>
        /// 安全将字符串转换为 decimal，如果转换失败返回默认值。
        /// </summary>
        /// <param name="input">要转换的字符串。</param>
        /// <param name="defaultValue">转换失败时返回的默认值（默认为 0）。</param>
        /// <returns>转换后的十进制数值或默认值。</returns>
        public static decimal ToDecimal(this string input, decimal defaultValue = 0)
            => decimal.TryParse(input, out var result) ? result : defaultValue;

        /// <summary>
        /// 对字符串进行URL编码（使用Uri.EscapeDataString）。
        /// </summary>
        /// <param name="input">要编码的字符串。</param>
        /// <returns>URL编码后的字符串。</returns>
        public static string UrlEncode(this string input)
            => Uri.EscapeDataString(input);

        /// <summary>
        /// 对URL编码字符串进行解码（使用Uri.UnescapeDataString）。
        /// </summary>
        /// <param name="input">要解码的字符串。</param>
        /// <returns>解码后的原始字符串。</returns>
        public static string UrlDecode(this string input)
            => Uri.UnescapeDataString(input);

        /// <summary>
        /// 移除所有空白字符（空格、制表符、换行等）
        /// </summary>
        /// <param name="input">要处理的输入字符串。</param>
        /// <returns>移除空白字符后的字符串。</returns>
        public static string RemoveWhitespace(this string input)
        {
            return string.IsNullOrEmpty(input) ? input : Regex.Replace(input, @"\s+", "");
        }

        /// <summary>
        ///  超长时裁剪并加“...”
        /// </summary>
        /// <param name="input">要裁剪的输入字符串。</param>
        /// <param name="length">最大允许长度。</param>
        /// <returns>裁剪后的字符串，如果超长则添加"..."。</returns>
        public static string TruncateWithEllipsis(this string input, int length)
        {
            if (string.IsNullOrEmpty(input) || length <= 0) return "";
            return input.Length <= length ? input : input.Substring(0, length) + "...";
        }

        /// <summary>
        /// 字符串重复 N 次
        /// </summary>
        /// <param name="input">要重复的字符串。</param>
        /// <param name="count">重复次数。</param>
        /// <returns>重复后的字符串。</returns>
        public static string Repeat(this string input, int count)
        {
            if (string.IsNullOrEmpty(input) || count <= 0) return string.Empty;
            return string.Concat(Enumerable.Repeat(input, count));
        }

        /// <summary>
        /// 移除非法文件名字符
        /// </summary>
        /// <param name="input">要处理的文件名。</param>
        /// <param name="replacement">替换非法字符的字符串（默认为"_"）。</param>
        /// <returns>安全的文件名。</returns>
        public static string ToSafeFileName(this string input, string replacement = "_")
        {
            var invalid = System.IO.Path.GetInvalidFileNameChars();
            foreach (var c in invalid)
            {
                input = input.Replace(c.ToString(), replacement);
            }
            return input;
        }

        /// <summary>
        /// 移除 HTML 标签。
        /// </summary>
        /// <param name="input">包含HTML的字符串。</param>
        /// <returns>移除HTML标签后的纯文本。</returns>
        public static string RemoveHtmlTags(this string input)
            => Regex.Replace(input, "<.*?>", "");

        /// <summary>
        /// 转义 SQL LIKE 模式中的特殊字符（包括 '[', '%', '_', '''）。
        /// </summary>
        /// <param name="input">包含要转义的 SQL LIKE 模式的字符串。</param>
        /// <returns>转义后的安全字符串，可直接用于 SQL LIKE 查询。</returns>
        public static string EscapeForSqlLike(this string input)
        {
            return input.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]").Replace("'", "''");
        }

        /// <summary>
        /// 使用正则表达式替换字符串中的匹配项。
        /// </summary>
        /// <param name="input">要进行替换操作的原始字符串。</param>
        /// <param name="pattern">用于匹配的正则表达式模式。</param>
        /// <param name="replacement">替换匹配项的字符串。</param>
        /// <returns>所有匹配项被替换后的新字符串。</returns>
        public static string ReplaceRegex(this string input, string pattern, string replacement)
        {
            return Regex.Replace(input, pattern, replacement);
        }

        /// <summary>
        /// 检查字符串是否可以解析为有效的 Guid。
        /// </summary>
        /// <param name="input">要检查的字符串。</param>
        /// <returns>如果字符串是有效的 Guid 格式则返回 true，否则返回 false。</returns>
        public static bool IsGuid(this string input)
            => Guid.TryParse(input, out _);

        /// <summary>
        /// 将字符串安全转换为 Guid 结构。
        /// </summary>
        /// <param name="input">要转换的字符串。</param>
        /// <returns>如果转换成功返回对应的 Guid 值，否则返回 Guid.Empty。</returns>
        public static Guid ToGuid(this string input)
            => Guid.TryParse(input, out var guid) ? guid : Guid.Empty;

        /// <summary>
        /// 将字符串转换为十六进制表示的字符串。
        /// </summary>
        /// <param name="input">要编码的原始字符串。</param>
        /// <returns>由十六进制字符组成的字符串，表示输入字符串的字节序列。</returns>
        public static string HexEncode(this string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        /// <summary>
        /// 十六进制字符串 TO 字符串
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static string HexDecode(this string hex)
        {
            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// 判断字符串是否全是中文
        /// </summary>
        /// <param name="input">要检查的字符串。</param>
        /// <returns>如果全是中文则返回 true，否则返回 false。</returns>>
        public static bool IsChinese(this string input)
        {
            return Regex.IsMatch(input, @"^[\u4e00-\u9fa5]+$");
        }

        /// <summary>
        /// 判断字符串是否包含中文
        /// </summary>
        /// <param name="input">要检查的字符串。</param>
        /// <returns>如果包含中文则返回 true，否则返回 false。</returns>
        public static bool ContainsChinese(this string input)
        {
            return Regex.IsMatch(input, @"[\u4e00-\u9fa5]");
        }

        #region 字符脱敏
        /// <summary>
        /// 字符串脱敏模式
        /// </summary>
        public enum StringMaskMode
        {
            /// <summary>保留头部字符</summary>
            KeepHead,
            /// <summary>保留尾部字符</summary>
            KeepTail,
            /// <summary>保留首尾字符</summary>
            KeepHeadAndTail,
            /// <summary>仅保留中间字符</summary>
            KeepMiddle,
            /// <summary>完全脱敏</summary>
            FullMask,
            /// <summary>电子邮件模式（保留@前后部分首尾）</summary>
            Email,
            /// <summary>身份证号模式（保留前3后4）</summary>
            IdCard,
            /// <summary>银行卡号模式（保留前4后4）</summary>
            BankCard
        }

        /// </summary>
        /// <param name="input">原始字符串</param>
        /// <param name="visibleHead">头部保留的可见字符数（默认4）</param>
        /// <param name="visibleTail">尾部保留的可见字符数（默认0）</param>
        /// <param name="maskChar">掩码字符（默认'*'）</param>
        /// <returns>脱敏后的字符串</returns>
        /// <example>
        /// "13800138000".Mask()       → "138*******"
        /// "张三".Mask(1, 1)          → "张*"
        /// "example@domain.com".Mask(3, 4) → "exa****@domain.com"
        /// </example>
        public static string Mask(
            this string input,
            int visibleHead = 4,
            int visibleTail = 0,
            char maskChar = '*')
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // 处理超出字符串长度的情况
            visibleHead = Math.Min(visibleHead, input.Length);
            visibleTail = Math.Min(visibleTail, input.Length - visibleHead);

            var head = input.Substring(0, visibleHead);
            var tail = visibleTail > 0 ? input.Substring(input.Length - visibleTail) : string.Empty;
            var maskedLength = input.Length - visibleHead - visibleTail;

            return maskedLength > 0
                ? $"{head}{new string(maskChar, maskedLength)}{tail}"
                : input;
        }

        /// <summary>
        /// 高级字符串脱敏处理
        /// </summary>
        /// <param name="input">原始字符串</param>
        /// <param name="mode">脱敏模式</param>
        /// <param name="maskChar">掩码字符（默认'*'）</param>
        /// <returns>脱敏后的字符串</returns>
        public static string Mask(
            this string input,
            StringMaskMode mode = StringMaskMode.KeepHead,
            char maskChar = '*')
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return mode switch
            {
                StringMaskMode.KeepHead =>
                    input.Length <= 4
                        ? $"{input.Substring(0, 1)}{new string(maskChar, input.Length - 1)}"
                        : $"{input.Substring(0, 4)}{new string(maskChar, input.Length - 4)}",

                StringMaskMode.KeepTail =>
                    input.Length <= 4
                        ? $"{new string(maskChar, input.Length - 1)}{input.Substring(input.Length - 1)}"
                        : $"{new string(maskChar, input.Length - 4)}{input.Substring(input.Length - 4)}",

                StringMaskMode.KeepHeadAndTail =>
                    input.Mask(visibleHead: 2, visibleTail: 2, maskChar: maskChar),

                StringMaskMode.KeepMiddle =>
                    input.Length <= 2
                        ? new string(maskChar, input.Length)
                        : $"{new string(maskChar, 2)}{input.Substring(2, input.Length - 4)}{new string(maskChar, 2)}",

                StringMaskMode.FullMask =>
                    new string(maskChar, input.Length),

                StringMaskMode.Email =>
                    input.Contains('@')
                        ? MaskEmail(input, maskChar)
                        : input.Mask(3, 4, maskChar),

                StringMaskMode.IdCard =>
                    input.Mask(3, 4, maskChar),

                StringMaskMode.BankCard =>
                    input.Mask(4, 4, maskChar),

                _ => input
            };
        }

        private static string MaskEmail(string email, char maskChar)
        {
            var parts = email.Split('@');
            return $"{parts[0].Mask(3, 0, maskChar)}@{parts[1].Mask(1, 0, maskChar)}";
        }
        #endregion

    }
}
