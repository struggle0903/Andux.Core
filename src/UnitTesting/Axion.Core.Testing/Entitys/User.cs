using Andux.Core.EfTrack;

namespace Andux.Core.Testing.Entity
{
    public class User: BaseEntity<int>
    {
        public string Name { get; set; } = null!;
        public int Age { get; set; }
    }
}
