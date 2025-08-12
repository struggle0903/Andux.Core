using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Andux.Core.EfTrack;

namespace Andux.Admin.Domain.Entitys
{
    /// <summary>
    /// 在线用户统计表
    /// </summary>
    [Table("sys_online_users")]
    public class AnduxOnlineUser : IEntity<Guid>
    {
        /// <summary>
        /// 主键ID
        /// 会话记录唯一标识
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// 关联用户ID
        /// 外键，指向用户表
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 导航属性，关联用户实体
        /// </summary>
        public AnduxUser User { get; set; } = null!;

        /// <summary>
        /// 当前活跃会话数量
        /// 表示该用户当前有多少个有效的会话（多端登录）
        /// </summary>
        public int ActiveSessionCount { get; set; }

        /// <summary>
        /// 最近登录时间
        /// 用户最近一次登录系统的时间
        /// </summary>
        public DateTime LastLoginTime { get; set; }

        /// <summary>
        /// 最后活动时间
        /// 用户最近一次操作时间，用于判断是否真正活跃
        /// </summary>
        public DateTime LastActivityTime { get; set; }

    }
}
