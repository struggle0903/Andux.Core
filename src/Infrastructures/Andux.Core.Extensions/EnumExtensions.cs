using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Andux.Core.Extensions
{
    /// <summary>
    /// 枚举扩展方法
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// 获取枚举值的 Description 特性描述（需配合 [Description("描述")] 使用）
        /// </summary>
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }

        /// <summary>
        /// 将枚举转换为字典（Key=枚举值，Value=描述）
        /// </summary>
        public static Dictionary<int, string> ToDictionary(this Enum value)
        {
            return Enum.GetValues(value.GetType())
                       .Cast<Enum>()
                       .ToDictionary(e => Convert.ToInt32(e), e => e.GetDescription());
        }

        /// <summary>
        /// 安全解析字符串到枚举（解析失败返回默认值）
        /// </summary>
        public static T ToEnum<T>(this string value, T defaultValue = default) where T : Enum
        {
            return Enum.TryParse(typeof(T), value, true, out var result) ? (T)result : defaultValue;
        }

        /// <summary>
        /// 检查枚举是否包含指定标志（适用于 [Flags] 枚举）
        /// </summary>
        public static bool HasFlag(this Enum value, Enum flag)
        {
            if (value.GetType() != flag.GetType())
                throw new ArgumentException("枚举类型不匹配");

            var valueInt = Convert.ToUInt64(value);
            var flagInt = Convert.ToUInt64(flag);
            return (valueInt & flagInt) == flagInt;
        }

        /// <summary>
        /// 获取枚举的所有值
        /// </summary>
        public static IEnumerable<T> GetValues<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// 将枚举值转换为Int（避免直接强转的装箱拆箱）
        /// </summary>
        public static int ToInt(this Enum value)
        {
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// 检查枚举值是否在定义范围内
        /// </summary>
        public static bool IsDefined(this Enum value)
        {
            return Enum.IsDefined(value.GetType(), value);
        }

        /// <summary>
        /// 获取枚举的 DisplayName 特性（需配合 [Display(Name="显示名")] 使用）
        /// </summary>
        public static string GetDisplayName(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DisplayAttribute>();
            return attribute?.Name ?? value.ToString();
        }

        /// <summary>
        /// 为位枚举添加标志（适用于 [Flags] 枚举）
        /// </summary>
        public static T AddFlag<T>(this Enum value, T flag) where T : Enum
        {
            var valueInt = Convert.ToUInt64(value);
            var flagInt = Convert.ToUInt64(flag);
            return (T)Enum.ToObject(typeof(T), valueInt | flagInt);
        }

        /// <summary>
        /// 从位枚举移除标志（适用于 [Flags] 枚举）
        /// </summary>
        public static T RemoveFlag<T>(this Enum value, T flag) where T : Enum
        {
            var valueInt = Convert.ToUInt64(value);
            var flagInt = Convert.ToUInt64(flag);
            return (T)Enum.ToObject(typeof(T), valueInt & ~flagInt);
        }

        /// <summary>
        /// 检查枚举值是否在 Flags 组合中（适用于多选枚举）
        /// </summary>
        public static bool IsInFlags(this Enum value, Enum flags)
        {
            if (!value.GetType().IsEnum || !flags.GetType().IsEnum)
                throw new ArgumentException("参数必须是枚举类型");

            var valueInt = Convert.ToUInt64(value);
            var flagsInt = Convert.ToUInt64(flags);
            return (flagsInt & valueInt) == valueInt;
        }

        /// <summary>
        /// 获取枚举的默认值（第一个定义的值）
        /// </summary>
        public static T DefaultValue<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().First();
        }

        /// <summary>
        /// 获取枚举的最大值（基于底层数值）
        /// </summary>
        public static T MaxValue<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().Max();
        }

        /// <summary>
        /// 获取枚举的最小值（基于底层数值）
        /// </summary>
        public static T MinValue<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().Min();
        }

        /// <summary>
        /// 检查枚举是否等于指定的任意一个值
        /// </summary>
        public static bool IsAnyOf<T>(this T value, params T[] values) where T : Enum
        {
            return values.Contains(value);
        }

        /// <summary>
        /// 获取枚举的上一个值（循环）
        /// </summary>
        public static T Previous<T>(this T value) where T : Enum
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            int currentIndex = values.IndexOf(value);
            int prevIndex = (currentIndex - 1 + values.Count) % values.Count;
            return values[prevIndex];
        }

        /// <summary>
        /// 获取枚举的下一个值（循环）
        /// </summary>
        public static T Next<T>(this T value) where T : Enum
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            int currentIndex = values.IndexOf(value);
            int nextIndex = (currentIndex + 1) % values.Count;
            return values[nextIndex];
        }

        /// <summary>
        /// 检查枚举值是否在指定的范围内（闭区间）
        /// </summary>
        public static bool IsInRange<T>(this T value, T min, T max) where T : Enum, IComparable
        {
            return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
        }

        /// <summary>
        /// 获取枚举的所有名称（字符串形式）
        /// </summary>
        public static List<string> GetNames<T>() where T : Enum
        {
            return Enum.GetNames(typeof(T)).ToList();
        }

    }
}
