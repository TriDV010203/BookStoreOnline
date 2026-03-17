using BookStoreOnline.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreOnline.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 0. Lấy tất cả đánh giá (dùng cho trang chủ tính sao trung bình)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAllReviews()
        {
            var reviews = await _context.ProductReviews
                .Include(r => r.User)
                .Select(r => new
                {
                    r.Id,
                    r.UserId,
                    UserName  = r.User != null ? r.User.FullName : "Ẩn danh",
                    r.BookId,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(reviews);
        }

        // 1. Lấy tất cả đánh giá của 1 sách
        [HttpGet("book/{bookId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetReviewsByBook(int bookId)
        {
            var reviews = await _context.ProductReviews
                .Where(r => r.BookId == bookId)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    r.UserId,
                    UserName = r.User != null ? r.User.FullName : "Ẩn danh",
                    r.BookId,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(reviews);
        }

        // 2. Kiểm tra xem user có thể đánh giá sách này không
        //    Điều kiện: đã có đơn hàng chứa sách đó VÀ chưa đánh giá lần nào
        [HttpGet("can-review")]
        public async Task<ActionResult<object>> CanReview([FromQuery] int userId, [FromQuery] int bookId)
        {
            // Kiểm tra đã mua (có OrderDetail với sách đó, Order của user)
            var hasPurchased = await _context.OrderDetails
                .AnyAsync(od =>
                    od.BookId == bookId &&
                    od.Order != null &&
                    od.Order.UserId == userId);

            // Kiểm tra đã review chưa
            var hasReviewed = await _context.ProductReviews
                .AnyAsync(r => r.UserId == userId && r.BookId == bookId);

            return Ok(new
            {
                canReview = hasPurchased && !hasReviewed,
                hasPurchased,
                hasReviewed
            });
        }

        // 3. Tạo đánh giá mới
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
        {
            // Validate rating
            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest("Rating phải từ 1 đến 5 sao.");

            // Kiểm tra đã mua chưa
            var hasPurchased = await _context.OrderDetails
                .AnyAsync(od =>
                    od.BookId == request.BookId &&
                    od.Order != null &&
                    od.Order.UserId == request.UserId);

            if (!hasPurchased)
                return BadRequest("Bạn chưa mua sản phẩm này, không thể đánh giá.");

            // Kiểm tra đã review chưa
            var alreadyReviewed = await _context.ProductReviews
                .AnyAsync(r => r.UserId == request.UserId && r.BookId == request.BookId);

            if (alreadyReviewed)
                return BadRequest("Bạn đã đánh giá sản phẩm này rồi.");

            var review = new ProductReview
            {
                UserId = request.UserId,
                BookId = request.BookId,
                Rating = request.Rating,
                Comment = request.Comment?.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.ProductReviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đánh giá thành công!", reviewId = review.Id });
        }
    }

    public class CreateReviewRequest
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
