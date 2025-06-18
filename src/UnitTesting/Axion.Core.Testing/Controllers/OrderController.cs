using Andux.Core.EfTrack;
using Andux.Core.EfTrack.Repository.Paged;
using Andux.Core.Testing.Entitys;
using Microsoft.AspNetCore.Mvc;

namespace Andux.Core.Testing.Controllers
{
    [ApiController]
    [Route("api/order")]
    public class OrderController : ControllerBase
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerRepository"></param>
        /// <param name="orderRepository"></param>
        /// <param name="orderItemRepository"></param>
        /// <param name="productRepository"></param>
        /// <param name="unitOfWork"></param>
        public OrderController(IRepository<Customer> customerRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<Product> productRepository,
            IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 初始化数据（新增测试数据）
        /// </summary>
        [HttpPost("init")]
        public async Task<IActionResult> InitData()
        {
            var customer = new Customer { Name = "Test Customer" };
            var product1 = new Product { Name = "Product A", Price = 10 };
            var product2 = new Product { Name = "Product B", Price = 20 };

            var order = new Order
            {
                Customer = customer,
                OrderDate = DateTime.UtcNow,
                Items = new List<OrderItem>
            {
                new OrderItem { Product = product1, Quantity = 2 },
                new OrderItem { Product = product2, Quantity = 1 }
            }
            };

            await _orderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "初始化数据完成", orderId = order.Id });
        }

        /// <summary>
        /// 查询订单（带分页 + 指定导航属性）
        /// </summary>
        [HttpGet("getPage")]
        public async Task<IActionResult> GetOrderAsync([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var pageParam = new BasePageParam { Page = page, Limit = limit };
            var pageResult = await _orderRepository.GetPagedWithIncludesAsync(pageParam, "Customer");

            return Ok(new PagedResult<Order>
            {
                TotalCount = pageResult.TotalCount,
                TotalPages = (int)Math.Ceiling(pageResult.TotalCount / (double)limit),
                Items = pageResult.Items
            });
        }

        /// <summary>
        /// 查询订单（带分页 + 指定导航属性）
        /// </summary>
        [HttpGet("getPages")]
        public async Task<IActionResult> GetOrder2Async([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var pageParam = new BasePageParam { Page = page, Limit = limit };
            var pageResult = await _orderRepository.GetPagedWithIncludesAsync(pageParam,  s => s.Customer);

            return Ok(new PagedResult<Order>
            {
                TotalCount = pageResult.TotalCount,
                TotalPages = (int)Math.Ceiling(pageResult.TotalCount / (double)limit),
                Items = pageResult.Items
            });
        }

        /// <summary>
        /// 详情
        /// </summary>
        [HttpGet("getDetail")]
        public async Task<IActionResult> GetAsync(long id)
        {
            var detail = await _orderRepository.GetByIdWithIncludesAsync(id, s => s.Customer);
            return Ok(detail);
        }

        /// <summary>
        /// 详情
        /// </summary>
        [HttpGet("getDetail2")]
        public async Task<IActionResult> Get2Async(long id)
        {
            var detail = await _orderRepository.GetByIdWithIncludesAsync(id, "Customer");
            return Ok(detail);
        }

        /// <summary>
        /// sum统计
        /// </summary>
        [HttpGet("sum")]
        public async Task<IActionResult> SumAsync(string name)
        {
            var sum = await _productRepository.SumAsync(
                oi => oi.Name == name,
                oi => oi.Price * 2
            );

            return Ok(sum);
        }

        /// <summary>
        /// 平均值
        /// </summary>
        [HttpGet("avg")]
        public async Task<IActionResult> AverageAsync(string name)
        {
            var avg = await _productRepository.AverageAsync(
                oi => oi.Name == name,
                oi => oi.Price * 3
            );

            return Ok(avg);
        }

        /// <summary>
        /// 最大值
        /// </summary>
        [HttpGet("max")]
        public async Task<IActionResult> MaxAsync(string name)
        {
            var avg = await _productRepository.MaxAsync(
                oi => oi.Name == name,
                oi => oi.Price
            );

            return Ok(avg);
        }

        /// <summary>
        /// 最大值
        /// </summary>
        [HttpGet("min")]
        public async Task<IActionResult> MinAsync(string name)
        {
            var avg = await _productRepository.MinAsync(
                oi => oi.Name == name,
                oi => oi.Price
            );

            return Ok(avg);
        }

        /// <summary>
        /// 去重
        /// </summary>
        [HttpGet("distinct")]
        public async Task<IActionResult> DistinctAsync()
        {
            // 查找订单 ID 大于 0 的不同客户 ID
            var customerIds = await _orderRepository.DistinctAsync(
                o => o.Id > 0,
                o => o.CustomerId
            );

            return Ok(customerIds);
        }

        /// <summary>
        /// 去重
        /// </summary>
        [HttpGet("groupBy")]
        public async Task<IActionResult> GroupByAsync()
        {
            // 分组统计每个产品的总数量
            var result = await _orderItemRepository.GroupByAsync(
                oi => oi.Quantity > 0,
                oi => oi.ProductId,
                g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity)
                }
            );

            return Ok(result);
        }

    }
}
