using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Andux.Core.EfTenant
{
    /// <summary>
    /// 租户db配置
    /// </summary>
    [Table("tenant_db_configs")]
    public class TenantDbConfig: TenantBaseEntity<long>
    {
        /// <summary>
        /// 租户ID
        /// </summary>
        public long TenantId { get; set; }

        /// <summary>
        /// 租户状态：0-禁用 1-启用
        /// </summary>
        public TenantStatusEnum Status { get; set; }

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnectionString { get; set; } = default!;

        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(500)]
        public string? Remark { get; set; }
    }
}
