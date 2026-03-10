using BookStoreOnline.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreOnline.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Lấy danh sách Đơn hàng (Kèm theo chi tiết từng cuốn sách bên trong)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders
                .Include(o => o.OrderDetails) // Lấy danh sách chi tiết
                .ThenInclude(od => od.Book)   // Lấy luôn thông tin cuốn sách trong chi tiết đó
                .ToListAsync();
        }

        // 2. Xem chi tiết 1 đơn hàng cụ thể
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)         // Kèm thông tin người mua
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return order;
        }

        // 3. Đặt hàng (Checkout)
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            // Tránh EF Core tự động tạo lại User và Book mới
            order.User = null;
            if (order.OrderDetails != null)
            {
                foreach (var detail in order.OrderDetails)
                {
                    detail.Book = null;
                }
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrder", new { id = order.Id }, order);
        }
    }
}