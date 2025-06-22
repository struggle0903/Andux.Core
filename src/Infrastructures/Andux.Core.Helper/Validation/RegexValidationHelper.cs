using System.Text.RegularExpressions;

namespace Andux.Core.Helper.Validation
{
    /// <summary>
    /// 正则表达式验证扩展类
    /// </summary>
    public static class RegexValidationHelper
    {
        /// <summary>
        /// 常用正则表达式模式
        /// </summary>
        public static class Patterns
        {
            public const string Email = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            public const string ChineseMobile = @"^1[3-9]\d{9}$";
            public const string ChineseIdCard = @"(^\d{15}$)|(^\d{17}(\d|X|x)$)";
            public const string UnifiedSocialCreditCode = @"^[0-9A-HJ-NPQRTUWXY]{2}\d{6}[0-9A-HJ-NPQRTUWXY]{10}$";
            public const string IPAddress = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$|^([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$";
            public const string Url = @"^(https?|ftp)://([^\s/$.?#].[^\s]*)$";
            public const string DateTimeFormat = @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}$";
            public const string ChinesePostalCode = @"^[1-9]\d{5}$";
            public const string ChineseLicensePlate = @"^[京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼使领][A-HJ-NP-Z][A-HJ-NP-Z0-9]{4,5}[A-HJ-NP-Z0-9挂学警港澳]$";
            public const string BankCardNumber = @"^[1-9]\d{15,18}$";
            public const string Username = @"^[a-zA-Z0-9_]{4,20}$";
            public const string StrongPassword = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[\s\S]{8,}$";
            public const string ChineseName = @"^[\u4e00-\u9fa5]{2,5}$";
            public const string WeChatId = @"^[a-zA-Z][-_a-zA-Z0-9]{5,19}$";
            public const string QQNumber = @"^[1-9]\d{4,11}$";
            public const string MacAddress = @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$";
            public const string Base64String = @"^[A-Za-z0-9+/]+={0,2}$";
            public const string HtmlTag = @"<([a-z]+)([^<]+)*(?:>(.*)<\/\1>|\s+\/>)";

            public const string LandlinePhone = @"^0\d{2,3}-?\d{7,8}$"; // 座机电话
            public const string Guid = @"^[{(]?[0-9A-Fa-f]{8}(-[0-9A-Fa-f]{4}){3}-[0-9A-Fa-f]{12}[)}]?$";
            public const string Money = @"^([1-9]\d{0,9}|0)(\.\d{1,2})?$"; // 金额（整数最多10位，小数最多2位）
            public const string OnlyChinese = @"^[\u4e00-\u9fa5]+$"; // 全中文
            public const string HexColor = @"^#?([a-fA-F0-9]{6}|[a-fA-F0-9]{3})$"; // 色号（#fff 或 #ffffff）
            public const string English = @"^[A-Za-z]+$"; // 全英文
            public const string Number = @"^\d+$"; // 全数字
            public const string NumberOrLetter = @"^[A-Za-z0-9]+$"; // 数字或字母
            public const string IPv4 = @"^(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)){3}$";
            public const string IPv6 = @"^([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$";
            public const string WeChatPayOrderNo = @"^[0-9]{18}$"; // 微信支付订单号
            public const string Time24Hour = @"^(?:[01]\d|2[0-3]):[0-5]\d$"; // 24小时制时间，如 23:59
            public const string Year = @"^\d{4}$";
            public const string Month = @"^(0?[1-9]|1[012])$"; // 1-12
            public const string Day = @"^([1-9]|[12]\d|3[01])$"; // 1-31
        }

        /// <summary>
        /// 验证字符串是否匹配指定正则表达式
        /// </summary>
        /// <param name="helper">ValidationHelper 实例</param>
        /// <param name="value">要验证的字符串</param>
        /// <param name="fieldName">字段名称（用于错误消息）</param>
        /// <param name="pattern">正则表达式模式</param>
        /// <param name="message">自定义错误消息（可选）</param>
        /// <param name="regexOptions">正则表达式选项（可选）</param>
        /// <returns>当前 ValidationHelper 实例（支持链式调用）</returns>
        public static ValidationHelper RegexMatch(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string pattern,
            string? message = null,
            RegexOptions regexOptions = RegexOptions.None)
        {
            if (string.IsNullOrEmpty(value)) return helper;

            if (!Regex.IsMatch(value, pattern, regexOptions))
            {
                helper.AddError(message ?? $"{fieldName}格式不符合要求");
            }
            return helper;
        }

        /// <summary>
        /// 验证邮箱地址
        /// </summary>
        public static ValidationHelper Email(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.Email,
                message ?? $"{fieldName}不是有效的邮箱地址");
        }
        /// <summary>
        /// 验证中国手机号
        /// </summary>
        public static ValidationHelper ChineseMobile(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.ChineseMobile,
                message ?? $"{fieldName}不是有效的手机号");
        }

        /// <summary>
        /// 验证中国身份证号码（支持18位和15位）
        /// </summary>
        public static ValidationHelper ChineseIdCard(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.ChineseIdCard,
                message ?? $"{fieldName}不是有效的身份证号码");
        }

        /// <summary>
        /// 验证统一社会信用代码
        /// </summary>
        public static ValidationHelper UnifiedSocialCreditCode(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.UnifiedSocialCreditCode,
                message ?? $"{fieldName}不是有效的统一社会信用代码");
        }

        /// <summary>
        /// 验证IP地址（IPv4或IPv6）
        /// </summary>
        public static ValidationHelper IPAddress(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.IPAddress,
                message ?? $"{fieldName}不是有效的IP地址");
        }

        /// <summary>
        /// 验证URL地址
        /// </summary>
        public static ValidationHelper Url(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.Url,
                message ?? $"{fieldName}不是有效的URL地址");
        }

        /// <summary>
        /// 验证日期时间格式（yyyy-MM-dd HH:mm:ss）
        /// </summary>
        public static ValidationHelper DateTimeFormat(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.DateTimeFormat,
                message ?? $"{fieldName}不是有效的日期时间格式（要求：yyyy-MM-dd HH:mm:ss）");
        }

        /// <summary>
        /// 验证邮政编码（中国）
        /// </summary>
        public static ValidationHelper ChinesePostalCode(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.ChinesePostalCode,
                message ?? $"{fieldName}不是有效的邮政编码");
        }

        /// <summary>
        /// 验证车牌号（中国）
        /// </summary>
        public static ValidationHelper ChineseLicensePlate(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.ChineseLicensePlate,
                message ?? $"{fieldName}不是有效的车牌号");
        }

        /// <summary>
        /// 验证银行卡号（国内通用16-19位）
        /// </summary>
        public static ValidationHelper BankCardNumber(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.BankCardNumber,
                message ?? $"{fieldName}不是有效的银行卡号");
        }

        /// <summary>
        /// 验证用户名（4-20位字母数字下划线）
        /// </summary>
        public static ValidationHelper Username(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.Username,
                message ?? $"{fieldName}必须是4-20位字母数字或下划线");
        }

        /// <summary>
        /// 验证密码强度（至少8位，含大小写字母和数字）
        /// </summary>
        public static ValidationHelper StrongPassword(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.StrongPassword,
                message ?? $"{fieldName}必须包含大小写字母和数字且至少8位");
        }

        /// <summary>
        /// 验证中文姓名（2-5个汉字）
        /// </summary>
        public static ValidationHelper ChineseName(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.ChineseName,
                message ?? $"{fieldName}必须是2-5个汉字");
        }

        /// <summary>
        /// 验证微信号（6-20位字母、数字、下划线或减号）
        /// </summary>
        public static ValidationHelper WeChatId(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.WeChatId,
                message ?? $"{fieldName}不是有效的微信号");
        }

        /// <summary>
        /// 验证QQ号（5-12位数字）
        /// </summary>
        public static ValidationHelper QQNumber(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.QQNumber,
                message ?? $"{fieldName}不是有效的QQ号");
        }

        /// <summary>
        /// 验证MAC地址
        /// </summary>
        public static ValidationHelper MacAddress(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.MacAddress,
                message ?? $"{fieldName}不是有效的MAC地址");
        }

        /// <summary>
        /// 验证Base64字符串
        /// </summary>
        public static ValidationHelper Base64String(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.Base64String,
                message ?? $"{fieldName}不是有效的Base64编码");
        }

        /// <summary>
        /// 验证HTML标签
        /// </summary>
        public static ValidationHelper HtmlTag(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.HtmlTag,
                message ?? $"{fieldName}包含不合法的HTML标签");
        }

        /// <summary>
        /// 验证GUID
        /// </summary>
        public static ValidationHelper Guid(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null) => helper.RegexMatch(value, fieldName, Patterns.Guid, message ?? $"{fieldName} 不是有效的 GUID");

        /// <summary>
        /// 验证金额（保留 2 位小数）
        /// </summary>
        public static ValidationHelper Money(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null) =>
            helper.RegexMatch(value, fieldName, Patterns.Money, message ?? $"{fieldName} 不是有效的金额格式");

        /// <summary>
        /// 验证座机电话
        /// </summary>
        public static ValidationHelper LandlinePhone(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.LandlinePhone,
                message ?? $"{fieldName} 不是有效的座机号码");
        }

        /// <summary>
        /// 全中文验证
        /// </summary>
        public static ValidationHelper OnlyChinese(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null) => helper.RegexMatch(value,fieldName,Patterns.OnlyChinese,message ?? $"{fieldName} 必须全为中文字符");
        
        /// <summary>
        /// 色号验证
        /// </summary>
        public static ValidationHelper HexColor(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.HexColor,
                message ?? $"{fieldName} 不是合法的颜色码（如 #FFFFFF）");
        }

        /// <summary>
        /// 验证是否为全英文
        /// </summary>
        public static ValidationHelper English(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.English,
                message ?? $"{fieldName} 必须是全英文字符");
        }

        /// <summary>
        /// 验证是否为全数字
        /// </summary>
        public static ValidationHelper Number(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.Number,
                message ?? $"{fieldName} 必须是纯数字");
        }

        /// <summary>
        /// 验证是否为数字或字母
        /// </summary>
        public static ValidationHelper NumberOrLetter(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.NumberOrLetter,
                message ?? $"{fieldName} 只能包含数字或字母");
        }

        /// <summary>
        /// 验证是否为合法 IPv4 地址
        /// </summary>
        public static ValidationHelper IPv4(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.IPv4,
                message ?? $"{fieldName} 不是合法的 IPv4 地址");
        }

        /// <summary>
        /// 验证是否为合法 IPv6 地址
        /// </summary>
        public static ValidationHelper IPv6(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.IPv6,
                message ?? $"{fieldName} 不是合法的 IPv6 地址");
        }

        /// <summary>
        /// 验证是否为微信支付订单号（18位数字）
        /// </summary>
        public static ValidationHelper WeChatPayOrderNo(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null)
        {
            return helper.RegexMatch(
                value,
                fieldName,
                Patterns.WeChatPayOrderNo,
                message ?? $"{fieldName} 不是合法的微信支付订单号");
        }

        /// <summary>
        /// 验证 24 小时制时间（HH:mm）
        /// </summary>
        public static ValidationHelper Time24Hour(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null) =>
            helper.RegexMatch(value, fieldName, Patterns.Time24Hour, message ?? $"{fieldName} 不是有效的 24 小时格式时间");

        /// <summary>
        /// 验证年份（4位数字）
        /// </summary>
        public static ValidationHelper Year(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null) =>
            helper.RegexMatch(value, fieldName, Patterns.Year, message ?? $"{fieldName} 不是有效的年份");

        /// <summary>
        /// 验证月份（01 - 12）
        /// </summary>
        public static ValidationHelper Month(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null) =>
            helper.RegexMatch(value, fieldName, Patterns.Month, message ?? $"{fieldName} 不是有效的月份");

        /// <summary>
        /// 验证日期（01 - 31）
        /// </summary>
        public static ValidationHelper Day(
            this ValidationHelper helper,
            string value,
            string fieldName,
            string? message = null) =>
            helper.RegexMatch(value, fieldName, Patterns.Day, message ?? $"{fieldName} 不是有效的日期");

    }
}
