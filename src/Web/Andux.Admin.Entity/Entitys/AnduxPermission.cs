using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Andux.Admin.Domain.Enums;
using Andux.Core.EfTrack;
using Microsoft.EntityFrameworkCore;

namespace Andux.Admin.Domain.Entitys
{
    /// <summary>
    /// 权限
    /// </summary>
    [Table("sys_permissions")]
    [Index(nameof(Name), IsUnique = true)]
    public class AnduxPermission : BaseEntity<Guid>
    {
        /// <summary>
        /// 权限名称（唯一标识）
        /// </summary>
        [Required]
        [MaxLength(50)]
        public required string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; } = null!;

        /// <summary>
        /// 权限类型（默认 "Api"）
        /// 常用类型：Api（接口权限）、Menu（菜单权限）、Data（数据权限）
        /// 便于区分不同类别的权限
        /// </summary>
        [Required]
        public PermissionType Type { get; set; }

        /// <summary>
        /// 资源地址（可选）
        /// 对于 API 权限：可以是接口路由，如 "/api/users/create"
        /// 对于菜单权限：可以是前端路由路径，如 "/users/create"
        /// 对于数据权限：可为数据资源标识
        /// </summary>
        public string? Resource { get; set; }

        /// <summary>
        /// 父级权限ID（可选）
        /// 用于构建权限树（层级结构），根节点的 ParentId 为 null
        /// </summary>
        public long? ParentId { get; set; }

        /// <summary>
        /// 排序值（越小越靠前）
        /// 用于同级权限的显示顺序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 是否启用（默认 true）
        /// false 表示逻辑停用，该权限不会生效
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        public ICollection<AnduxRolePermission> RolePermissions { get; set; } = new List<AnduxRolePermission>();
    }
}
