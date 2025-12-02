using Andux.Core.EfTrack.Entities;
using Andux.Core.EfTrack;
using System.ComponentModel.DataAnnotations.Schema;

namespace Andux.Core.Testing.Entitys
{
    public class TestUser : BaseEntity<int>, IProject
    {
        public string Name { get; set; } = null!;
        public int Age { get; set; }
        public long? ProjectId { get; set; }
    }
}
