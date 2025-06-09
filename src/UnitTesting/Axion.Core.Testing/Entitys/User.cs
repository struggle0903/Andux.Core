using Andux.Core.EfTrack;

namespace Andux.Core.Testing.Entitys
{
    public class User: BaseEntity<int>
    {
        public string Name { get; set; } = null!;
        public int Age { get; set; }
    }
}
