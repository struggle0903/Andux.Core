using System.ComponentModel.DataAnnotations.Schema;

namespace Andux.Admin.Domain.Entitys
{
    [Table("sys_test")]
    public class TestEntity
    {
        public int Id { get; set; }
        public string TestName { get; set; }

    }

    [Table("sys_test2_andy")]
    public class TestEntity2
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Sex { get; set; }
    }
}
