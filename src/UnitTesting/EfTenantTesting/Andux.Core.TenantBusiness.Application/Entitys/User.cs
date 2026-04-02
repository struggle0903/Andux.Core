using Andux.Core.EfTenant;

namespace Andux.Core.TenantTesting.Application.Entitys
{
    public class User : TenantBaseEntity<long>, ITenantProject
    {
        public long Id { get; set; }

        public string Name { get; set; } = default!;

        public long? ProjectId { get; set; }
    }
}
