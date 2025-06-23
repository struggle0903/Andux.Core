using System.Text;
using System.Text.RegularExpressions;

namespace Andux.Core.Extensions
{
    /// <summary>
    /// 数值类型扩展方法
    /// </summary>
    public static class NumberExtensions
    {
        /// <summary>
        /// 将数值转换为指定位数的四舍五入值
        /// </summary>
        /// <param name="value">要处理的值</param>
        /// <param name="decimals">保留的小数位数</param>
        /// <returns>四舍五入后的值</returns>
        public static double Round(this double value, int decimals = 2)
            => Math.Round(value, decimals);

        /// <summary>
        /// 将数值向上取整
        /// </summary>
        /// <param name="value">要处理的值</param>
        /// <returns>向上取整后的值</returns>
        public static double Ceiling(this double value)
            => Math.Ceiling(value);

        /// <summary>
        /// 将数值向下取整
        /// </summary>
        /// <param name="value">要处理的值</param>
        /// <returns>向下取整后的值</returns>
        public static double Floor(this double value)
            => Math.Floor(value);

        /// <summary>
        /// 检查数值是否在指定范围内
        /// </summary>
        /// <param name="value">要检查的值</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <param name="inclusive">是否包含边界值</param>
        /// <returns>如果在范围内返回true，否则返回false</returns>
        public static bool IsBetween(this IComparable value, IComparable min, IComparable max, bool inclusive = true)
        {
            return inclusive
                ? value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0
                : value.CompareTo(min) > 0 && value.CompareTo(max) < 0;
        }

        /// <summary>
        /// 将数值转换为百分比字符串
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <param name="decimals">保留的小数位数</param>
        /// <returns>百分比字符串</returns>
        public static string ToPercent(this double value, int decimals = 2)
            => (value * 100).Round(decimals) + "%";

        /// <summary>
        /// 将数值转换为中文大写金额
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>中文大写金额字符串</returns>
        public static string ToChineseRMB(this decimal value)
        {
            if (value == 0)
                return "零元整";

            string[] numChars = { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };
            string[] unitChars = { "", "拾", "佰", "仟", "万", "拾", "佰", "仟", "亿", "拾", "佰", "仟" };

            string str = value.ToString("#L#E#D#C#K#E#D#C#J#E#D#C#I#E#D#C#H#E#D#C#G#E#D#C#F#E#D#C#.0B0A");
            string result = Regex.Replace(str, @"((?<=-|^)[^1-9]*)|((?'z'0)[0A-E]*((?=[1-9])|(?'-z'(?=[F-L\.]|$))))|((?'b'[F-L])(?'z'0)[0A-L]*((?=[1-9])|(?'-z'(?=[\.]|$))))",
                "${b}${z}");

            return Regex.Replace(result, ".", m =>
            {
                string c = m.Value;
                if (c == "A") return "分";
                if (c == "B") return "角";
                if (c == "C") return "元";
                if (c == "D") return "拾";
                if (c == "E") return "佰";
                if (c == "F") return "仟";
                if (c == "G") return "万";
                if (c == "H") return "拾";
                if (c == "I") return "佰";
                if (c == "J") return "仟";
                if (c == "K") return "亿";
                if (c == "L") return "拾";
                if (c == ".") return "";
                return numChars[int.Parse(c)] + unitChars[m.Index];
            }) + (value.ToString().Contains(".") ? "" : "整");
        }

        /// <summary>
        /// 将数值转换为文件大小字符串（如 1.23 MB）
        /// </summary>
        /// <param name="bytes">字节数</param>
        /// <param name="decimals">保留的小数位数</param>
        /// <returns>文件大小字符串</returns>
        public static string ToFileSize(this long bytes, int decimals = 2)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len.Round(decimals)} {sizes[order]}";
        }

        /// <summary>
        /// 将数值限制在指定范围内
        /// </summary>
        /// <param name="value">要限制的值</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>限制后的值</returns>
        public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0) return min;
            if (value.CompareTo(max) > 0) return max;
            return value;
        }

        /// <summary>
        /// 检查数值是否为偶数
        /// </summary>
        /// <param name="value">要检查的值</param>
        /// <returns>如果是偶数返回true，否则返回false</returns>
        public static bool IsEven(this int value)
            => value % 2 == 0;

        /// <summary>
        /// 检查数值是否为奇数
        /// </summary>
        /// <param name="value">要检查的值</param>
        /// <returns>如果是奇数返回true，否则返回false</returns>
        public static bool IsOdd(this int value)
            => value % 2 != 0;

        /// 将数值转换为罗马数字
        /// </summary>
        /// <param name="number">要转换的数字（1-3999）</param>
        /// <returns>罗马数字字符串</returns>
        public static string ToRoman(this int number)
        {
            if (number < 1 || number > 3999)
                throw new ArgumentOutOfRangeException(nameof(number), "输入数字必须在1-3999之间");

            var romanNumerals = new Dictionary<int, string>
                {
                    { 1000, "M" },
                    { 900, "CM" },
                    { 500, "D" },
                    { 400, "CD" },
                    { 100, "C" },
                    { 90, "XC" },
                    { 50, "L" },
                    { 40, "XL" },
                    { 10, "X" },
                    { 9, "IX" },
                    { 5, "V" },
                    { 4, "IV" },
                    { 1, "I" }
                };

            var result = new StringBuilder();

            foreach (var item in romanNumerals)
            {
                while (number >= item.Key)
                {
                    result.Append(item.Value);
                    number -= item.Key;
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 将数值转换为十六进制字符串
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>十六进制字符串</returns>
        public static string ToHex(this int value)
            => value.ToString("X");

        /// <summary>
        /// 将数值转换为二进制字符串
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>二进制字符串</returns>
        public static string ToBinary(this int value)
            => Convert.ToString(value, 2);

        /// <summary>
        /// 计算数值的绝对值
        /// </summary>
        /// <param name="value">要计算的值</param>
        /// <returns>绝对值</returns>
        public static double Abs(this double value)
            => Math.Abs(value);

        /// <summary>
        /// 计算数值的平方
        /// </summary>
        /// <param name="value">要计算的值</param>
        /// <returns>平方值</returns>
        public static double Square(this double value)
            => value * value;

        /// <summary>
        /// 计算数值的平方根
        /// </summary>
        /// <param name="value">要计算的值</param>
        /// <returns>平方根</returns>
        public static double Sqrt(this double value)
            => Math.Sqrt(value);

        /// <summary>
        /// 计算数值的幂次方
        /// </summary>
        /// <param name="value">底数</param>
        /// <param name="exponent">指数</param>
        /// <returns>计算结果</returns>
        public static double Pow(this double value, double exponent)
            => Math.Pow(value, exponent);

        /// <summary>
        /// 将角度转换为弧度
        /// </summary>
        /// <param name="degrees">角度值</param>
        /// <returns>弧度值</returns>
        public static double ToRadians(this double degrees)
            => degrees * (Math.PI / 180);

        /// <summary>
        /// 将弧度转换为角度
        /// </summary>
        /// <param name="radians">弧度值</param>
        /// <returns>角度值</returns>
        public static double ToDegrees(this double radians)
            => radians * (180 / Math.PI);

        /// <summary>
        /// 检查数值是否近似等于另一个值（考虑浮点数精度问题）
        /// </summary>
        /// <param name="value">当前值</param>
        /// <param name="other">比较值</param>
        /// <param name="tolerance">容差</param>
        /// <returns>如果近似相等返回true，否则返回false</returns>
        public static bool Approximately(this double value, double other, double tolerance = 0.0001)
            => Math.Abs(value - other) < tolerance;

        /// <summary>
        /// 将数值转换为可空类型
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>可空数值</returns>
        public static int? ToNullable(this int value)
            => value;

        /// <summary>
        /// 将数值转换为布尔值（非零为true）
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>布尔值</returns>
        public static bool ToBool(this int value)
            => value != 0;

        /// <summary>
        /// 将数值转换为时间跨度（毫秒）
        /// </summary>
        /// <param name="milliseconds">毫秒数</param>
        /// <returns>TimeSpan对象</returns>
        public static TimeSpan ToTimeSpan(this int milliseconds)
            => TimeSpan.FromMilliseconds(milliseconds);

        /// <summary>
        /// 从 Unix 时间戳（秒级）转换为 DateTime
        /// </summary>
        public static DateTime FromUnixTimestamp(this long timestamp)
        {
            return DateTime.UnixEpoch.AddSeconds(timestamp).ToLocalTime();
        }

    }
}
