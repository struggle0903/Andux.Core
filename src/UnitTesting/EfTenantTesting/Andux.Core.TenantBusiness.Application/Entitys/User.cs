using Andux.Core.EfTenant;

namespace Andux.Core.TenantTesting.Application.Entitys
{
    public class User : BaseEntity<long>, IProject
    {
        public long Id { get; set; }

        public string Name { get; set; } = default!;

        public long? ProjectId { get; set; }
    }
}
