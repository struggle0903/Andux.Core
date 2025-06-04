using Nevo.Core.EfFramework.Entities;

namespace EfFramework.Test.Entity
{
    public class Role : BaseEntity<int>
    {
        public string Name { get; set; } = null!;
    }
}
