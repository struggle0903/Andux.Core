using Andux.Core.EfTrack;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Andux.Admin.Domain.Entitys
{
    /// <summary>
    /// 用户
    /// </summary>
    [Table("sys_users")]
    [Index(nameof(Account), IsUnique = true)]
    public class AnduxUser : BaseEntity<Guid>
    {
        /// <summary>
        /// 登录账户
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Account { get; set; } = null!;

        /// <summary>
        /// 系统显示用户名
        /// </summary>
        [Required]
        [MaxLength(20)]
        public required string UserName { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// hash 密码
        /// </summary>
        [Required]
        [MaxLength(200)]
        public required string PasswordHash { get; set; }

        /// <summary>
        /// 秘密盐
        /// </summary>
        [Required]
        [MaxLength(50)]
        public required string PasswordSalt { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        public ICollection<AnduxUserRole> UserRoles { get; set; } = new List<AnduxUserRole>();
    }
}
