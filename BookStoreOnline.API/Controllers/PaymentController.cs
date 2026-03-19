using BookStoreOnline.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreOnline.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(ApplicationDbContext context, IConfiguration config, ILogger<PaymentController> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        // ─── GET /api/payment/status/{orderId} ──────────────────────────────────
        [HttpGet("status/{orderId}")]
        public async Task<IActionResult> GetPaymentStatus(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            return Ok(new
            {
                isPaid = order.OrderStatus == "Paid",
                status = order.OrderStatus
            });
        }

        // ─── POST /api/payment/expire/{orderId} ──────────────────────────────────
        // MVC gọi khi đồng hồ đếm ngược hết – tự động hủy đơn chờ thanh toán
        [HttpPost("expire/{orderId}")]
        public async Task<IActionResult> ExpireOrder(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            // Chỉ hủy nếu vẫn đang chờ
            if (order.OrderStatus == "AwaitingPayment")
            {
                // Hoàn trả tồn kho
                var details = await _context.OrderDetails
                    .Where(d => d.OrderId == orderId)
                    .ToListAsync();

                foreach (var d in details)
                {
                    var book = await _context.Books.FindAsync(d.BookId);
                    if (book != null) book.StockQuantity += d.Quantity;
                }

                order.OrderStatus = "Cancelled";
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đơn hàng #{OrderId} đã hủy do hết thời gian thanh toán QR.", orderId);
                return Ok(new { cancelled = true });
            }

            return Ok(new { cancelled = false, status = order.OrderStatus });
        }

        // ─── GET /api/payment/test ────────────────────────────────────────────────
        // Dùng để test ngrok còn kết nối được đến API không
        [HttpGet("test")]
        public IActionResult Test()
        {
            _logger.LogInformation("✅ Payment test endpoint được gọi thành công.");
            return Ok(new { ok = true, time = DateTime.Now, message = "API Payment đang hoạt động!" });
        }

        // ─── POST /api/payment/webhook ────────────────────────────────────────────
        // Sepay gọi endpoint này (qua ngrok) sau khi phát hiện giao dịch
        [HttpPost("webhook")]
        public async Task<IActionResult> SepayWebhook([FromBody] SepayWebhookPayload payload)
        {
            try
            {
                // Log toàn bộ payload để debug
                _logger.LogInformation(
                    "Webhook nhận: Id={Id}, TransferType={Type}, Amount={Amount}, Content='{Content}', Description='{Desc}'",
                    payload.Id, payload.TransferType, payload.TransferAmount,
                    payload.Content, payload.Description);

                // ── Auth đã bỏ để tránh block Sepay ──
                // (URL ngrok đã là bí mật, không cần thêm auth)

                // 2. Chỉ xử lý giao dịch tiền vào
                if (!string.Equals(payload.TransferType, "in", StringComparison.OrdinalIgnoreCase))
                    return Ok("Bỏ qua giao dịch tiền ra.");

                // 3. Tìm mã đơn hàng trong nội dung – kiểm tra cả Content và Description
                var content = payload.Content ?? "";
                var description = payload.Description ?? "";
                var searchText = string.IsNullOrWhiteSpace(content) ? description : content;

                _logger.LogInformation("Tìm orderId trong: '{Text}'", searchText);
                int? orderId = ExtractOrderId(searchText);

                // Nếu content không tìm được thì thử description
                if (orderId == null && searchText != description)
                    orderId = ExtractOrderId(description);

                if (orderId == null)
                {
                    _logger.LogWarning("Không tìm thấy orderId trong nội dung: '{Content}' / '{Desc}'",
                        content, description);
                    return Ok("Không tìm thấy mã đơn hàng trong nội dung.");
                }

                _logger.LogInformation("Tìm thấy orderId = {OrderId}", orderId);

                // 4. Tìm đơn hàng đang chờ thanh toán
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.OrderStatus == "AwaitingPayment");

                if (order == null)
                {
                    _logger.LogWarning("Đơn hàng #{OrderId} không tồn tại hoặc không ở trạng thái AwaitingPayment.", orderId);
                    return Ok("Đơn hàng không tồn tại hoặc đã được xử lý.");
                }

                // 5. Kiểm tra trùng giao dịch
                var txId = payload.ReferenceCode ?? payload.Id.ToString();
                var duplicate = await _context.Payments.AnyAsync(p => p.TransactionId == txId);
                if (duplicate)
                {
                    _logger.LogWarning("Giao dịch {TxId} đã được xử lý.", txId);
                    return Ok("Giao dịch đã được xử lý trước đó.");
                }

                // 6. Ghi payment record
                _context.Payments.Add(new Payment
                {
                    OrderId = order.Id,
                    Amount = (decimal)payload.TransferAmount,
                    Content = searchText,
                    TransactionId = txId,
                    CreatedAt = DateTime.Now
                });

                // 7. Cập nhật trạng thái đơn hàng
                order.OrderStatus = "Paid";
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Đơn hàng #{OrderId} đã thanh toán thành công. Tx={TxId}", orderId, txId);
                return Ok("Thanh toán xác nhận thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xử lý webhook");
                return StatusCode(500, $"Lỗi xử lý webhook: {ex.Message}");
            }
        }

        // ─── Helper: trích xuất orderId từ nội dung chuyển khoản ───────────────
        private static int? ExtractOrderId(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;

            // Ưu tiên: DH5, DH 5, #5, donhang5, order5
            var patterns = new[]
            {
                @"(?i)DH\s*(\d+)",          // DH5, DH 5 → format user đang dùng
                @"#(\d+)",                   // #5
                @"(?i)don\s*hang\s*(\d+)",   // donhang5
                @"(?i)order\s*(\d+)"         // order5
            };

            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(content, pattern);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int id))
                    return id;
            }
            return null;
        }
    }

    // ─── DTO nhận từ Sepay ────────────────────────────────────────────────────────
    public class SepayWebhookPayload
    {
        public long Id { get; set; }
        public string? Gateway { get; set; }
        public string? TransactionDate { get; set; }
        public string? AccountNumber { get; set; }
        public string? Content { get; set; }
        public string? TransferType { get; set; }
        public double TransferAmount { get; set; }
        public double Accumulated { get; set; }
        public string? ReferenceCode { get; set; }
        public string? Description { get; set; }
        public string? ApiKey { get; set; }
    }
}