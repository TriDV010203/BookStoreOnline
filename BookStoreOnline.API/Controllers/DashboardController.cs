using BookStoreOnline.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreOnline.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /api/dashboard/revenue?year=2026
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue([FromQuery] int? year)
        {
            int targetYear = year ?? DateTime.Now.Year;

            // Lấy tất cả đơn hàng không bị huỷ trong năm chỉ định, kèm chi tiết
            var orders = await _context.Orders
                .Where(o => o.OrderStatus != "Cancelled" && o.OrderDate.Year == targetYear)
                .Include(o => o.OrderDetails!)
                    .ThenInclude(od => od.Book)
                        .ThenInclude(b => b!.Category)
                .ToListAsync();

            // -- Doanh thu & lợi nhuận theo tháng --
            var monthlyRevenue = Enumerable.Range(1, 12).Select(m =>
            {
                var monthOrders = orders.Where(o => o.OrderDate.Month == m).ToList();
                var revenue = monthOrders
                    .SelectMany(o => o.OrderDetails!)
                    .Sum(od => od.UnitPrice * od.Quantity);
                var cost = monthOrders
                    .SelectMany(o => o.OrderDetails!)
                    .Sum(od => (od.Book?.ImportPrice ?? 0) * od.Quantity);
                return new
                {
                    month = m,
                    revenue,
                    profit = revenue - cost
                };
            }).ToList();

            // -- Thống kê theo danh mục (top 8) --
            var categoryStats = orders
                .SelectMany(o => o.OrderDetails!)
                .Where(od => od.Book?.Category != null)
                .GroupBy(od => od.Book!.Category!.Name)
                .Select(g => new
                {
                    categoryName = g.Key,
                    soldQty = g.Sum(od => od.Quantity),
                    revenue = g.Sum(od => od.UnitPrice * od.Quantity)
                })
                .OrderByDescending(c => c.soldQty)
                .Take(8)
                .ToList();

            // -- Tổng hợp --
            var allDetails = orders.SelectMany(o => o.OrderDetails!).ToList();
            var totalRevenue = allDetails.Sum(od => od.UnitPrice * od.Quantity);
            var totalCost = allDetails.Sum(od => (od.Book?.ImportPrice ?? 0) * od.Quantity);
            var totalProfit = totalRevenue - totalCost;
            var totalOrders = orders.Count;

            return Ok(new
            {
                year = targetYear,
                totalRevenue,
                totalProfit,
                totalOrders,
                monthlyRevenue,
                categoryStats
            });
        }
    }
}
