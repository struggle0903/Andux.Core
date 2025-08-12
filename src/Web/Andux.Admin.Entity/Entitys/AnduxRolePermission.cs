using Andux.Core.EfTrack;
using System.ComponentModel.DataAnnotations.Schema;

namespace Andux.Admin.Domain.Entitys
{
    /// <summary>
    /// 角色权限
    /// </summary>
    [Table("sys_role_permissions")]
    public class AnduxRolePermission : BaseEntity<Guid>
    {
        public Guid RoleId { get; set; }
        public AnduxRole Role { get; set; } = null!;

        public Guid PermissionId { get; set; }
        public AnduxPermission Permission { get; set; } = null!;
    }
}
