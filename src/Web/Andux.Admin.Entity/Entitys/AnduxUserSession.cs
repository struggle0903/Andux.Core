using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Andux.Core.EfTrack;

namespace Andux.Admin.Domain.Entitys
{
    /// <summary>
    /// 用户会话表
    /// </summary>
    [Table("sys_user_session")]
    public class AnduxUserSession: IEntity<Guid>
    {
        /// <summary>
        /// 主键ID
        /// 会话记录唯一标识
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// 关联用户ID
        /// 外键，指向用户表中的用户
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 导航属性，关联用户实体
        /// </summary>
        public AnduxUser User { get; set; } = null!;

        /// <summary>
        /// 会话唯一标识
        /// 一般对应 Token 或 Session ID，用于区分不同登录设备或终端
        /// </summary>
        public string SessionId { get; set; } = null!;

        /// <summary>
        /// 登录时间
        /// 用户开始本次会话的时间
        /// </summary>
        public DateTime LoginTime { get; set; }

        /// <summary>
        /// 最近活动时间
        /// 用户在此会话中的最后一次操作时间，用于判断是否在线或超时
        /// </summary>
        public DateTime LastActivityTime { get; set; }

        /// <summary>
        /// 登录IP地址
        /// 记录用户登录时的IP，方便安全审计和异常检测
        /// </summary>
        public string IpAddress { get; set; } = null!;

        /// <summary>
        /// 设备信息
        /// 记录客户端设备信息，如浏览器类型、操作系统、设备型号等（可选）
        /// </summary>
        public string? DeviceInfo { get; set; }

        /// <summary>
        /// 会话是否处于活跃状态
        /// true 表示用户未登出且会话有效
        /// false 表示用户已登出或会话被踢出
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 会话过期时间
        /// 会话自动失效时间，超时后需重新登录（可选）
        /// </summary>
        public DateTime? ExpireTime { get; set; }

    }
}
