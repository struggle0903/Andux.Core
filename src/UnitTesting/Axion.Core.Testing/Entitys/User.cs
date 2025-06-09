using Andux.Core.EfTrack;
using Andux.Core.EfTrack.Entities;

namespace Andux.Core.Testing.Entitys
{
    public class User: BaseEntity<int>, IProject
    {
        public string Name { get; set; } = null!;
        public int Age { get; set; }
        public long? ProjectId { get; set; }
    }
}
