using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Andux.Core.Helper.Validation
{
    /// <summary>
    /// 验证结果
    /// </summary>
    public sealed class ValidationResult
    {
        public bool IsValid { get; internal set; }
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
    /// 数据验证帮助类（实例方式）
    /// </summary>
    public class ValidationHelper
    {
        private readonly List<System.ComponentModel.DataAnnotations.ValidationResult> _validationResults = new();

        /// <summary>
        /// 验证是否通过
        /// </summary>
        public bool IsValid => !_validationResults.Any();

        /// <summary>
        /// 所有错误信息
        /// </summary>
        public IEnumerable<string> ErrorMessages => _validationResults.Select(r => r.ErrorMessage);

        /// <summary>
        /// 验证对象的数据注解
        /// </summary>
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
        /// 必填验证
        /// </summary>
        public ValidationHelper Required(string value, string fieldName, string message = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                AddError(message ?? $"{fieldName}不能为空");
            }
            return this;
        }

        /// <summary>
        /// 数值范围验证
        /// </summary>
        public ValidationHelper Range(int value, string fieldName, int min, int max, string message = null)
        {
            if (value < min || value > max)
            {
                AddError(message ?? $"{fieldName}必须在{min}到{max}之间");
            }
            return this;
        }

        /// <summary>
        /// 字符串长度验证
        /// </summary>
        public ValidationHelper Length(string value, string fieldName, int min, int max, string message = null)
        {
            if (value == null) return this;

            if (value.Length < min || value.Length > max)
            {
                AddError(message ?? $"{fieldName}长度必须在{min}到{max}之间");
            }
            return this;
        }

        /// <summary>
        /// 邮箱格式验证
        /// </summary>
        public ValidationHelper Email(string value, string fieldName, string message = null)
        {
            if (string.IsNullOrWhiteSpace(value)) return this;

            if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                AddError(message ?? $"{fieldName}格式不正确");
            }
            return this;
        }

        /// <summary>
        /// 手机号验证(中国)
        /// </summary>
        public ValidationHelper Phone(string value, string fieldName, string message = null)
        {
            if (string.IsNullOrWhiteSpace(value)) return this;

            if (!Regex.IsMatch(value, @"^1[3-9]\d{9}$"))
            {
                AddError(message ?? $"{fieldName}格式不正确");
            }
            return this;
        }

        /// <summary>
        /// 自定义验证
        /// </summary>
        public ValidationHelper Must(Func<bool> condition, string message)
        {
            if (!condition())
            {
                AddError(message);
            }
            return this;
        }

        /// <summary>
        /// 添加错误
        /// </summary>
        private void AddError(string message)
        {
            _validationResults.Add(new System.ComponentModel.DataAnnotations.ValidationResult(message));
        }

        /// <summary>
        /// 获取验证结果
        /// </summary>
        public ValidationResult ToResult()
        {
            return new ValidationResult
            {
                IsValid = IsValid,
                Errors = ErrorMessages.ToList()
            };
        }

        /// <summary>
        /// 如果验证失败则抛出异常
        /// </summary>
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
        public static ValidationHelper Create() => new ValidationHelper();

        /// <summary>
        /// 快速验证对象的数据注解
        /// </summary>
        public static ValidationResult Validate(object obj, bool validateAllProperties = true)
        {
            return Create()
                .ValidateDataAnnotations(obj, validateAllProperties)
                .ToResult();
        }

        /// <summary>
        /// 快速验证并抛出异常
        /// </summary>
        public static void ValidateAndThrow(object obj, bool validateAllProperties = true)
        {
            Validate(obj, validateAllProperties).ThrowIfInvalid();
        }

        /// <summary>
        /// 验证字符串不为空
        /// </summary>
        public static ValidationResult Required(string value, string fieldName, string message = null)
        {
            return Create().Required(value, fieldName, message).ToResult();
        }

        /// <summary>
        /// 验证数值范围
        /// </summary>
        public static ValidationResult Range(int value, string fieldName, int min, int max, string message = null)
        {
            return Create().Range(value, fieldName, min, max, message).ToResult();
        }

        /// <summary>
        /// 验证字符串长度
        /// </summary>
        public static ValidationResult Length(string value, string fieldName, int min, int max, string message = null)
        {
            return Create().Length(value, fieldName, min, max, message).ToResult();
        }

        /// <summary>
        /// 验证邮箱格式
        /// </summary>
        public static ValidationResult Email(string value, string fieldName, string message = null)
        {
            return Create().Email(value, fieldName, message).ToResult();
        }

        /// <summary>
        /// 验证手机号格式
        /// </summary>
        public static ValidationResult Phone(string value, string fieldName, string message = null)
        {
            return Create().Phone(value, fieldName, message).ToResult();
        }

        /// <summary>
        /// 自定义验证
        /// </summary>
        public static ValidationResult Must(Func<bool> condition, string message)
        {
            return Create().Must(condition, message).ToResult();
        }
    }

}
