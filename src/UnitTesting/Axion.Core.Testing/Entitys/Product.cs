namespace Andux.Core.Testing.Entitys
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }

        // 导航属性
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
