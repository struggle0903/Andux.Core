using Andux.Core.EfTrack;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Andux.Admin.Domain.Entitys
{
    /// <summary>
    /// 审计日志
    /// </summary>
    [Table("sys_audit_logs")]
    public class AnduxAuditLog : BaseEntity<Guid>
    {
        /// <summary>
        /// 接口名称
        /// </summary>
        [Required]
        [MaxLength(300)]
        public string ApiLabel { get; set; } = string.Empty;

        /// <summary>
        /// 控制器名称
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string ControllerName { get; set; } = string.Empty;

        /// <summary>
        /// 接口地址
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string ApiPath { get; set; } = string.Empty;

        /// <summary>
        /// 冗余字段-真实姓名/用户名
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string RealName { get; set; } = string.Empty;

        /// <summary>
        /// 状态码
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        [MaxLength(256)]
        public string RemoteIp { get; set; } = string.Empty;

        /// <summary>
        /// 国家
        /// </summary>
        [MaxLength(50)]
        public string Country { get; set; } = string.Empty;

        /// <summary>
        /// 省份
        /// </summary>
        [MaxLength(50)]
        public string Province { get; set; } = string.Empty;

        /// <summary>
        /// 城市
        /// </summary>
        [MaxLength(50)]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// 网络服务商
        /// </summary>
        [MaxLength(50)]
        public string Isp { get; set; } = string.Empty;

        /// <summary>
        /// 浏览器
        /// </summary>
        [MaxLength(200)]
        public string Browser { get; set; } = string.Empty;

        /// <summary>
        /// 浏览器信息
        /// </summary>
        [MaxLength(200)]
        public string BrowserInfo { get; set; } = string.Empty;

        /// <summary>
        /// 操作系统
        /// </summary>
        [MaxLength(256)]
        public string Os { get; set; } = string.Empty;

        /// <summary>
        /// 耗时（毫秒）
        /// </summary>
        public long ElapsedMilliseconds { get; set; }

        /// <summary>
        /// 操作状态
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// 请求方式
        /// </summary>
        [MaxLength(50)]
        public string HttpMethod { get; set; } = string.Empty;

        /// <summary>
        /// 请求参数
        /// </summary>
        [MaxLength(int.MaxValue)]
        public string RequestParam { get; set; } = string.Empty;

        /// <summary>
        /// 响应结果
        /// </summary>
        [MaxLength(int.MaxValue)]
        public string ReturnResult { get; set; } = string.Empty;
    }
}
