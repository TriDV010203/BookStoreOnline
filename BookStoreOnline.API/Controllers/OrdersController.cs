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

        // 1. Lấy danh sách Đơn hàng (Dành cho Admin)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        // 2. Lấy đơn hàng của 1 user
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByUser(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        // 3. Xem chi tiết 1 đơn hàng cụ thể
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return order;
        }

        // 4. Đặt hàng (Checkout) — trừ StockQuantity của sách
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            order.User = null;

            if (order.OrderDetails != null)
            {
                foreach (var detail in order.OrderDetails)
                {
                    detail.Book = null;

                    var book = await _context.Books.FindAsync(detail.BookId);
                    if (book == null)
                        return BadRequest($"Không tìm thấy sách ID {detail.BookId}.");

                    if (book.StockQuantity < detail.Quantity)
                        return BadRequest($"Sách \"{book.Title}\" không đủ số lượng trong kho (còn {book.StockQuantity}).");

                    book.StockQuantity -= detail.Quantity;
                }
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrder", new { id = order.Id }, order);
        }

        // 5. Cập nhật trạng thái đơn hàng — nếu hủy thì hoàn trả tồn kho
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound("Không tìm thấy đơn hàng.");

            var validStatuses = new[] { "Pending", "AwaitingPayment", "Paid", "Processing", "Shipped", "Delivered", "Cancelled" };
            if (!validStatuses.Contains(request.Status))
                return BadRequest("Trạng thái không hợp lệ.");

            // Nếu chuyển sang Cancelled → hoàn trả tồn kho
            if (request.Status == "Cancelled" && order.OrderStatus != "Cancelled")
            {
                foreach (var detail in order.OrderDetails!)
                {
                    var book = await _context.Books.FindAsync(detail.BookId);
                    if (book != null)
                        book.StockQuantity += detail.Quantity;
                }
            }

            order.OrderStatus = request.Status;
            await _context.SaveChangesAsync();
            return Ok("Cập nhật trạng thái thành công.");
        }
    }

    public class UpdateOrderStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}