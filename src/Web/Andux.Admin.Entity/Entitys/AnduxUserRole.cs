using Andux.Core.EfTrack;
using System.ComponentModel.DataAnnotations.Schema;

namespace Andux.Admin.Domain.Entitys
{
    /// <summary>
    /// 用户角色
    /// </summary>
    [Table("sys_user_roles")]
    public class AnduxUserRole : BaseEntity<Guid>
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public Guid UserId { get; set; }
        public AnduxUser User { get; set; } = null!;

        /// <summary>
        /// 角色id
        /// </summary>
        public Guid RoleId { get; set; }
        public AnduxRole Role { get; set; } = null!;
    }
}
