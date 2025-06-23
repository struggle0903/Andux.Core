namespace Andux.Core.Testing.Model
{
    /// <summary>
    /// 商品实体类
    /// </summary>
    public class ProductModel
    {
        /// <summary>
        /// 商品ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 商品价格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ProductModel(int id, string name, decimal price)
        {
            Id = id;
            Name = name;
            Price = price;
        }

        // 可选：重写ToString方便调试
        public override string ToString()
            => $"{Id}: {Name} (¥{Price})";
    }
}
