using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Andux.Core.Helper.Validation
{
    /// <summary>
    /// 验证结果
    /// </summary>
    public sealed class ValidationResult
    {
        /// <summary>
        /// 是否验证通过
        /// </summary>
        public bool IsValid { get; internal set; }

        /// <summary>
        /// 异常消息集合
        /// </summary>
        public IReadOnlyList<string> Errors { get; internal set; }

        /// <summary>
        /// 如果验证失败则抛出异常
        /// </summary>
        public void ThrowIfInvalid()
        {
            if (!IsValid)
            {
                throw new ValidationException(string.Join(Environment.NewLine, Errors));
            }
        }
    }

    /// <summary>
    /// 验证辅助类，提供链式验证方法
    /// </summary>
    public class ValidationHelper
    {
        private readonly List<System.ComponentModel.DataAnnotations.ValidationResult> _validationResults = new();

        /// <summary>
        /// 获取验证是否通过
        /// </summary>
        /// <value>如果没有任何验证错误返回true，否则返回false</value>
        public bool IsValid => !_validationResults.Any();

        /// <summary>
        /// 获取所有错误信息
        /// </summary>
        /// <value>包含所有验证错误消息的枚举集合</value>
        public IEnumerable<string> ErrorMessages => _validationResults.Select(r => r.ErrorMessage);

        /// <summary>
        /// 验证对象的数据注解属性
        /// </summary>
        /// <param name="obj">要验证的对象实例</param>
        /// <param name="validateAllProperties">true验证所有属性，false仅验证Required属性</param>
        /// <returns>当前ValidationHelper实例（支持链式调用）</returns>
        public ValidationHelper ValidateDataAnnotations(object obj, bool validateAllProperties = true)
        {
            if (obj == null)
            {
                AddError("验证对象不能为null");
                return this;
            }

            var context = new ValidationContext(obj);
            var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
                obj,
                context,
                results,
                validateAllProperties);

            _validationResults.AddRange(results);
            return this;
        }

        /// <summary>
        /// 验证字符串是否为必填项
        /// </summary>
        /// <param name="value">要验证的字符串值</param>
        /// <param name="fieldName">字段名称（用于错误消息）</param>
        /// <param name="message">自定义错误消息（可选）</param>
        /// <returns>当前ValidationHelper实例（支持链式调用）</returns>
        public ValidationHelper Required(string value, string fieldName, string? message = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                AddError(message ?? $"{fieldName}不能为空");
            }
            return this;
        }

        /// <summary>
        /// 验证整数值是否在指定范围内
        /// </summary>
        /// <param name="value">要验证的整数值</param>
        /// <param name="fieldName">字段名称（用于错误消息）</param>
        /// <param name="min">允许的最小值（包含）</param>
        /// <param name="max">允许的最大值（包含）</param>
        /// <param name="message">自定义错误消息（可选）</param>
        /// <returns>当前ValidationHelper实例（支持链式调用）</returns>
        public ValidationHelper Range(int value, string fieldName, int min, int max, string? message = null)
        {
            if (value < min || value > max)
            {
                AddError(message ?? $"{fieldName}必须在{min}到{max}之间");
            }
            return this;
        }

        /// <summary>
        /// 验证字符串长度是否在指定范围内
        /// </summary>
        /// <param name="value">要验证的字符串</param>
        /// <param name="fieldName">字段名称（用于错误消息）</param>
        /// <param name="min">最小长度（包含）</param>
        /// <param name="max">最大长度（包含）</param>
        /// <param name="message">自定义错误消息（可选）</param>
        /// <returns>当前ValidationHelper实例（支持链式调用）</returns>
        public ValidationHelper Length(string value, string fieldName, int min, int max, string? message = null)
        {
            if (value == null) return this;

            if (value.Length < min || value.Length > max)
            {
                AddError(message ?? $"{fieldName}长度必须在{min}到{max}之间");
            }
            return this;
        }

        /// <summary>
        /// 验证字符串是否符合邮箱格式
        /// </summary>
        /// <param name="value">要验证的邮箱地址</param>
        /// <param name="fieldName">字段名称（用于错误消息）</param>
        /// <param name="message">自定义错误消息（可选）</param>
        /// <returns>当前ValidationHelper实例（支持链式调用）</returns>
        public ValidationHelper Email(string value, string fieldName, string? message = null)
        {
            if (string.IsNullOrWhiteSpace(value)) return this;

            if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                AddError(message ?? $"{fieldName}格式不正确");
            }
            return this;
        }

        /// <summary>
        /// 验证字符串是否符合中国手机号格式
        /// </summary>
        /// <param name="value">要验证的手机号码</param>
        /// <param name="fieldName">字段名称（用于错误消息）</param>
        /// <param name="message">自定义错误消息（可选）</param>
        /// <returns>当前ValidationHelper实例（支持链式调用）</returns>
        public ValidationHelper Phone(string value, string fieldName, string? message = null)
        {
            if (string.IsNullOrWhiteSpace(value)) return this;

            if (!Regex.IsMatch(value, @"^1[3-9]\d{9}$"))
            {
                AddError(message ?? $"{fieldName}格式不正确");
            }
            return this;
        }

        /// <summary>
        /// 自定义验证条件
        /// </summary>
        /// <param name="condition">验证条件函数，返回true表示验证通过</param>
        /// <param name="message">验证失败时的错误消息</param>
        /// <returns>当前ValidationHelper实例（支持链式调用）</returns>
        public ValidationHelper Must(Func<bool> condition, string message)
        {
            if (!condition())
            {
                AddError(message);
            }
            return this;
        }

        /// <summary>
        /// 添加验证错误
        /// </summary>
        /// <param name="message">错误消息</param>
        public void AddError(string message)
        {
            _validationResults.Add(new System.ComponentModel.DataAnnotations.ValidationResult(message));
        }

        /// <summary>
        /// 获取最终验证结果
        /// </summary>
        /// <returns>包含验证状态和错误列表的ValidationResult对象</returns>
        public ValidationResult ToResult()
        {
            return new ValidationResult
            {
                IsValid = IsValid,
                Errors = ErrorMessages.ToList()
            };
        }

        /// <summary>
        /// 如果验证失败则抛出ValidationException异常
        /// </summary>
        /// <exception cref="ValidationException">当验证失败时抛出</exception>
        public void ThrowIfInvalid()
        {
            ToResult().ThrowIfInvalid();
        }
    }

    /// <summary>
    /// 静态验证器
    /// </summary>
    public static class Validator
    {
        /// <summary>
        /// 创建验证器实例
        /// </summary>
        /// <returns>返回一个新的ValidationHelper实例</returns>
        public static ValidationHelper Create() => new ValidationHelper();

        /// <summary>
        /// 快速验证对象的数据注解
        /// </summary>
        /// <param name="obj">要验证的对象</param>
        /// <param name="validateAllProperties">是否验证所有属性（true）或仅验证Required属性（false）</param>
        /// <returns>验证结果</returns>
        public static ValidationResult Validate(object obj, bool validateAllProperties = true)
        {
            return Create()
                .ValidateDataAnnotations(obj, validateAllProperties)
                .ToResult();
        }

        /// <summary>
        /// 快速验证并抛出异常
        /// </summary>
        /// <param name="obj">要验证的对象</param>
        /// <param name="validateAllProperties">是否验证所有属性（true）或仅验证Required属性（false）</param>
        public static void ValidateAndThrow(object obj, bool validateAllProperties = true)
        {
            Validate(obj, validateAllProperties).ThrowIfInvalid();
        }

        /// <summary>
        /// 验证字符串不为空
        /// </summary>
        /// <param name="value">要验证的字符串值</param>
        /// <param name="fieldName">字段名称（用于错误消息）</param>
        /// <param name="message">自定义错误消息（可选）</param>
        /// <returns>验证结果</returns>
        public static ValidationResult Required(string value, string fieldName, string? message = null)
        {
            return Create().Required(value, fieldName, message).ToResult();
        }

        /// <summary>
        /// 验证数值范围
        /// </summary>
        /// <param name="value">要验证的数值</param>
        /// <param name="fieldName">字段名称（用于错误消息）</param>
        /// <param name="min">最小值（包含）</param>
        /// <param name="max">最大值（包含）</param>
        /// <param name="message">自定义错误消息（可选）</param>
        /// <returns>验证结果</returns>
        public static ValidationResult Range(int value, string fieldName, int min, int max, string? message = null)
        {
            return Create().Range(value, fieldName, min, max, message).ToResult();
        }

        /// <summary>
        /// 验证字符串长度
        /// </summary>
        /// <param name="value">要验证的字符串</param>
        /// <param name="fieldName">字段名称（用于错误消息）</param>
        /// <param name="min">最小长度</param>
        /// <param name="max">最大长度</param>
        /// <param name="message">自定义错误消息（可选）</param>
        /// <returns>验证结果</returns>
        public static ValidationResult Length(string value, string fieldName, int min, int max, string? message = null)
        {
            return Create().Length(value, fieldName, min, max, message).ToResult();
        }

        /// <summary>
        /// 验证邮箱格式
        /// </summary>
        /// <param name="value">要验证的邮箱地址</param>
        /// <param name="fieldName">字段名称（用于错误消息）</param>
        /// <param name="message">自定义错误消息（可选）</param>
        /// <returns>验证结果</returns>
        public static ValidationResult Email(string value, string fieldName, string? message = null)
        {
            return Create().Email(value, fieldName, message).ToResult();
        }

        /// <summary>
        /// 验证手机号格式
        /// </summary>
        /// <param name="value">要验证的手机号码</param>
        /// <param name="fieldName">字段名称（用于错误消息）</param>
        /// <param name="message">自定义错误消息（可选）</param>
        /// <returns>验证结果</returns>
        public static ValidationResult Phone(string value, string fieldName, string? message = null)
        {
            return Create().Phone(value, fieldName, message).ToResult();
        }

        /// <summary>
        /// 自定义验证
        /// </summary>
        /// <param name="condition">验证条件函数，返回true表示验证通过</param>
        /// <param name="message">验证失败时的错误消息</param>
        /// <returns>验证结果</returns>
        public static ValidationResult Must(Func<bool> condition, string message)
        {
            return Create().Must(condition, message).ToResult();
        }

    }
}
