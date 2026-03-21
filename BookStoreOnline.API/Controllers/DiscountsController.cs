using BookStoreOnline.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreOnline.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DiscountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/discounts — Admin: lấy tất cả
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Discount>>> GetDiscounts()
        {
            return await _context.Discounts
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        // GET: api/discounts/active — Lấy các discount đang hiệu lực
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<Discount>>> GetActiveDiscounts()
        {
            var now = DateTime.Now;
            return await _context.Discounts
                .Where(d => d.IsActive && d.StartDate <= now && (d.EndDate == null || d.EndDate >= now))
                .ToListAsync();
        }

        // GET: api/discounts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Discount>> GetDiscount(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null) return NotFound();
            return discount;
        }

        // GET: api/discounts/for-book/{bookId} — Lấy % giảm cao nhất áp dụng cho sách
        [HttpGet("for-book/{bookId}")]
        public async Task<ActionResult> GetDiscountForBook(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null) return NotFound();

            var now = DateTime.Now;
            var activeDiscounts = await _context.Discounts
                .Where(d => d.IsActive && d.StartDate <= now && (d.EndDate == null || d.EndDate >= now))
                .ToListAsync();

            decimal bestPercent = 0;
            foreach (var d in activeDiscounts)
            {
                if (d.ApplyToAll)
                {
                    if (d.DiscountPercent > bestPercent) bestPercent = d.DiscountPercent;
                }
                else if (!string.IsNullOrEmpty(d.CategoryIds))
                {
                    var catIds = d.CategoryIds.Split(',').Select(s => int.TryParse(s.Trim(), out int val) ? val : 0);
                    if (catIds.Contains(book.CategoryId) && d.DiscountPercent > bestPercent)
                        bestPercent = d.DiscountPercent;
                }
            }

            return Ok(new { discountPercent = bestPercent });
        }

        // GET: api/discounts/for-category/{categoryId}
        [HttpGet("for-category/{categoryId}")]
        public async Task<ActionResult> GetDiscountForCategory(int categoryId)
        {
            var now = DateTime.Now;
            var activeDiscounts = await _context.Discounts
                .Where(d => d.IsActive && d.StartDate <= now && (d.EndDate == null || d.EndDate >= now))
                .ToListAsync();

            decimal bestPercent = 0;
            foreach (var d in activeDiscounts)
            {
                if (d.ApplyToAll)
                {
                    if (d.DiscountPercent > bestPercent) bestPercent = d.DiscountPercent;
                }
                else if (!string.IsNullOrEmpty(d.CategoryIds))
                {
                    var catIds = d.CategoryIds.Split(',').Select(s => int.TryParse(s.Trim(), out int val) ? val : 0);
                    if (catIds.Contains(categoryId) && d.DiscountPercent > bestPercent)
                        bestPercent = d.DiscountPercent;
                }
            }

            return Ok(new { discountPercent = bestPercent });
        }

        // POST: api/discounts
        [HttpPost]
        public async Task<ActionResult<Discount>> CreateDiscount(Discount discount)
        {
            discount.CreatedAt = DateTime.Now;
            // Nếu có EndDate, set về cuối ngày (23:59:59) để bao gồm toàn bộ ngày đó
            if (discount.EndDate.HasValue)
                discount.EndDate = discount.EndDate.Value.Date.AddDays(1).AddSeconds(-1);
            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDiscount), new { id = discount.Id }, discount);
        }

        // PUT: api/discounts/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDiscount(int id, Discount discount)
        {
            if (id != discount.Id) return BadRequest();
            _context.Entry(discount).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Discounts.Any(d => d.Id == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        // DELETE: api/discounts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiscount(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null) return NotFound();
            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
