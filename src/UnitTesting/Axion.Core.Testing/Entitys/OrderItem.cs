namespace Andux.Core.Testing.Entitys
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        // 导航属性
        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
