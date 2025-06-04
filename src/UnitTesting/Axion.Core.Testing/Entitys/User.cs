using Axion.Core.EfTrack.Entities;
using Nevo.Core.EfFramework.Entities;

namespace EfFramework.Test.Entity
{
    public class User: BaseEntity<int>
    {
        public string Name { get; set; } = null!;
        public int Age { get; set; }
    }
}
