namespace Andux.Core.Testing.Entitys
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }

        // 外键
        public int CustomerId { get; set; }

        // 导航属性
        public Customer Customer { get; set; } = null!;
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
