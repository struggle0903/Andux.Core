using Andux.Core.EfTenant;

namespace Andux.Core.TenantTesting.Application.Entitys
{
    public class User : TenantBaseEntity<long>, ITenantProject
    {
        public string Name { get; set; } = default!;

        public long? ProjectId { get; set; }
    }
}
