using Andux.Core.EfTrack;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Andux.Admin.Domain.Entitys
{
    /// <summary>
    /// 角色
    /// </summary>
    [Table("sys_roles")]
    [Index(nameof(Name), IsUnique = true)]
    public class AnduxRole : BaseEntity<Guid>
    {
        /// <summary>
        /// 角色名称
        /// </summary>
        [Required]
        [MaxLength(50)]
        public required string Name { get; set; }

        /// <summary>
        /// 角色描述
        /// </summary>
        public string? Description { get; set; } = null!;

        /// <summary>
        /// 是否系统角色
        /// </summary>
        public bool IsSystem { get; set; } = false;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        public ICollection<AnduxUserRole> UserRoles { get; set; } = new List<AnduxUserRole>();
        public ICollection<AnduxRolePermission> RolePermissions { get; set; } = new List<AnduxRolePermission>();
    }
}
