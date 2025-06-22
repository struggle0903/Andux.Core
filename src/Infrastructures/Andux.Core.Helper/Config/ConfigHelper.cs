using Microsoft.Extensions.Configuration;

namespace Andux.Core.Helper.Config
{
    /// <summary>
    /// 配置读取帮助类
    /// 封装了对IConfiguration的常用操作
    /// </summary>
    public class ConfigHelper
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 初始化配置帮助类
        /// </summary>
        /// <param name="configuration">IConfiguration实例</param>
        public ConfigHelper(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// 获取配置值（字符串）
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值或默认值</returns>
        public string GetString(string key, string defaultValue = null)
        {
            return _configuration[key] ?? defaultValue;
        }

        /// <summary>
        /// 获取配置值（整数）
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值或默认值</returns>
        public int GetInt(string key, int defaultValue = 0)
        {
            return int.TryParse(_configuration[key], out var result) ? result : defaultValue;
        }

        /// <summary>
        /// 获取配置值（布尔值）
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值或默认值</returns>
        public bool GetBool(string key, bool defaultValue = false)
        {
            return bool.TryParse(_configuration[key], out var result) ? result : defaultValue;
        }

        /// <summary>
        /// 获取配置值（浮点数）
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值或默认值</returns>
        public double GetDouble(string key, double defaultValue = 0.0)
        {
            return double.TryParse(_configuration[key], out var result) ? result : defaultValue;
        }

        /// <summary>
        /// 获取配置值（时间跨度）
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值或默认值</returns>
        public TimeSpan GetTimeSpan(string key, TimeSpan defaultValue = default)
        {
            return TimeSpan.TryParse(_configuration[key], out var result) ? result : defaultValue;
        }

        /// <summary>
        /// 获取配置值（枚举值）
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值或默认值</returns>
        public T GetEnum<T>(string key, T defaultValue = default) where T : struct
        {
            return Enum.TryParse<T>(_configuration[key], true, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// 获取配置节
        /// </summary>
        /// <param name="key">配置节键</param>
        /// <returns>IConfigurationSection实例</returns>
        public IConfigurationSection GetSection(string key)
        {
            return _configuration.GetSection(key);
        }

        /// <summary>
        /// 获取配置并绑定到对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="key">配置键</param>
        /// <returns>绑定后的对象实例</returns>
        public T Get<T>(string key = null) where T : class, new()
        {
            var instance = new T();
            if (string.IsNullOrEmpty(key))
            {
                _configuration.Bind(instance);
            }
            else
            {
                _configuration.GetSection(key).Bind(instance);
            }
            return instance;
        }

        /// <summary>
        /// 获取连接字符串
        /// </summary>
        /// <param name="name">连接字符串名称</param>
        /// <returns>连接字符串</returns>
        public string GetConnectionString(string name)
        {
            return _configuration.GetConnectionString(name);
        }

    }
}
