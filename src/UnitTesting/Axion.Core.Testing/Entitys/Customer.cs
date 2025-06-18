using StackExchange.Redis;

namespace Andux.Core.Testing.Entitys
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // 导航属性
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
