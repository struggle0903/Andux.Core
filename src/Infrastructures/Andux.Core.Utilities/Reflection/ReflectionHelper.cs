using System.Reflection;

namespace Andux.Core.Helper.Reflection
{
    /// <summary>
    /// 反射帮助类
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// 获取对象指定属性的值（支持公有和私有实例属性）。
        /// </summary>
        /// <param name="obj">目标对象实例。</param>
        /// <param name="propertyName">属性名称。</param>
        /// <returns>属性值，若属性不存在则返回 null。</returns>
        public static object? GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException(nameof(propertyName));

            var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return prop?.GetValue(obj);
        }

        /// <summary>
        /// 设置对象指定属性的值（支持公有和私有实例属性）。
        /// </summary>
        /// <param name="obj">目标对象实例。</param>
        /// <param name="propertyName">属性名称。</param>
        /// <param name="value">待设置的属性值。</param>
        public static void SetPropertyValue(object obj, string propertyName, object? value)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException(nameof(propertyName));

            var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop == null)
                throw new ArgumentException($"属性 '{propertyName}' 不存在于类型 {obj.GetType().FullName} 中。");

            prop.SetValue(obj, value);
        }

        /// <summary>
        /// 获取对象指定字段的值（支持公有和私有实例字段）。
        /// </summary>
        /// <param name="obj">目标对象实例。</param>
        /// <param name="fieldName">字段名称。</param>
        /// <returns>字段值，若字段不存在则返回 null。</returns>
        public static object? GetFieldValue(object obj, string fieldName)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));

            var field = obj.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(obj);
        }

        /// <summary>
        /// 动态调用对象实例的指定方法（支持公有和私有实例方法）。
        /// </summary>
        /// <param name="obj">目标对象实例。</param>
        /// <param name="methodName">方法名称。</param>
        /// <param name="parameters">方法参数数组。</param>
        /// <returns>方法返回值，若方法不存在则返回 null。</returns>
        public static object? InvokeMethod(object obj, string methodName, params object[] parameters)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (string.IsNullOrEmpty(methodName)) throw new ArgumentNullException(nameof(methodName));

            var method = obj.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
                throw new ArgumentException($"方法 '{methodName}' 不存在于类型 {obj.GetType().FullName} 中。");

            return method.Invoke(obj, parameters);
        }

        /// <summary>
        /// 获取类型的所有属性名称。
        /// </summary>
        /// <param name="type">目标类型。</param>
        /// <param name="includePrivate">是否包含私有属性，默认 false。</param>
        /// <returns>属性名称集合。</returns>
        public static IEnumerable<string> GetPropertyNames(Type type, bool includePrivate = false)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var flags = BindingFlags.Instance | BindingFlags.Public;
            if (includePrivate) flags |= BindingFlags.NonPublic;

            return type.GetProperties(flags).Select(p => p.Name);
        }

        /// <summary>
        /// 获取泛型类型参数的第一个类型（如 List&lt;T&gt; 中的 T）。
        /// </summary>
        /// <param name="type">目标类型。</param>
        /// <returns>泛型参数类型，若不是泛型则返回 null。</returns>
        public static Type? GetGenericTypeArgument(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return type.IsGenericType ? type.GetGenericArguments().FirstOrDefault() : null;
        }

        /// <summary>
        /// 创建指定类型的实例（支持传递构造函数参数）。
        /// </summary>
        /// <param name="type">目标类型。</param>
        /// <param name="args">构造函数参数数组。</param>
        /// <returns>新创建的对象实例。</returns>
        public static object? CreateInstance(Type type, params object[] args)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return Activator.CreateInstance(type, args);
        }

        /// <summary>
        /// 判断指定类型是否包含指定名称的属性。
        /// </summary>
        /// <param name="type">目标类型。</param>
        /// <param name="propertyName">属性名称。</param>
        /// <returns>如果包含该属性返回 true，否则返回 false。</returns>
        public static bool HasProperty(Type type, string propertyName)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException(nameof(propertyName));

            return type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) != null;
        }

        /// <summary>
        /// 获取类型的所有方法名称。
        /// </summary>
        /// <param name="type">目标类型。</param>
        /// <param name="includePrivate">是否包含私有方法，默认 false。</param>
        /// <returns>方法名称集合。</returns>
        public static IEnumerable<string> GetMethodNames(Type type, bool includePrivate = false)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var flags = BindingFlags.Instance | BindingFlags.Public;
            if (includePrivate) flags |= BindingFlags.NonPublic;

            return type.GetMethods(flags).Select(m => m.Name);
        }

    }
}
