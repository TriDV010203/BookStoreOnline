using BookStoreOnline.MVC.Models;
using BookStoreOnline.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BookStoreOnline.MVC.Controllers
{
    public class CartController : Controller
    {
        private readonly IApiService _apiService;
        private readonly IConfiguration _config;
        private const string CartSessionKey = "ShoppingCart";

        public CartController(IApiService apiService, IConfiguration config)
        {
            _apiService = apiService;
            _config = config;
        }

        // ── Lấy giỏ hàng từ Session ──────────────────────────────────────────
        private List<CartItemViewModel> GetCartFromSession()
        {
            var json = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(json))
                return new List<CartItemViewModel>();
            return JsonSerializer.Deserialize<List<CartItemViewModel>>(json) ?? new();
        }

        private void SaveCartToSession(List<CartItemViewModel> cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }

        // ── GET /Cart ─────────────────────────────────────────────────────────
        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            var vm = new CartViewModel { Items = cart };
            return View(vm);
        }

        // ── POST /Cart/Add ────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Add(int bookId, int quantity = 1, string? returnUrl = null)
        {
            var book = await _apiService.GetBookByIdAsync(bookId);
            if (book == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sách.";
                return RedirectToAction("Index", "Home");
            }

            if (book.StockQuantity <= 0)
            {
                TempData["ErrorMessage"] = "Sách này đã hết hàng.";
                return RedirectToAction("Index", "Home");
            }

            var cart = GetCartFromSession();
            var existing = cart.FirstOrDefault(c => c.BookId == bookId);

            if (existing != null)
            {
                int newQty = existing.Quantity + quantity;
                existing.Quantity = Math.Min(newQty, book.StockQuantity);
            }
            else
            {
                cart.Add(new CartItemViewModel
                {
                    BookId = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    ImageUrl = book.ImageUrl,
                    UnitPrice = book.Price,
                    StockQuantity = book.StockQuantity,
                    Quantity = Math.Min(quantity, book.StockQuantity)
                });
            }

            SaveCartToSession(cart);
            TempData["SuccessMessage"] = $"Đã thêm \"{book.Title}\" vào giỏ hàng!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // ── POST /Cart/Remove ─────────────────────────────────────────────────
        [HttpPost]
        public IActionResult Remove(int bookId)
        {
            var cart = GetCartFromSession();
            cart.RemoveAll(c => c.BookId == bookId);
            SaveCartToSession(cart);
            TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            return RedirectToAction("Index");
        }

        // ── POST /Cart/UpdateQuantity ──────────────────────────────────────────
        [HttpPost]
        public IActionResult UpdateQuantity(int bookId, int quantity)
        {
            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(c => c.BookId == bookId);
            if (item != null)
            {
                if (quantity <= 0)
                    cart.RemoveAll(c => c.BookId == bookId);
                else
                    item.Quantity = Math.Min(quantity, item.StockQuantity);
            }
            SaveCartToSession(cart);
            return RedirectToAction("Index");
        }

        // ── GET /Cart/Checkout ────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Checkout()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login", "Account", new { returnUrl = "/Cart/Checkout" });

            var cart = GetCartFromSession();
            if (cart.Count == 0)
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index");
            }

            var vm = new CheckoutViewModel { Items = cart };
            return View(vm);
        }

        // ── POST /Cart/Checkout ────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login", "Account");

            var cart = GetCartFromSession();
            model.Items = cart;

            if (!ModelState.IsValid)
                return View(model);

            if (cart.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Giỏ hàng của bạn đang trống.");
                return View(model);
            }

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdStr, out int userId))
            {
                TempData["ErrorMessage"] = "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }

            var paymentMethod = model.PaymentMethod == "QR" ? "QR" : "COD";
            var (success, message, orderId) = await _apiService.PlaceOrderAsync(userId, model.ShippingAddress, cart, paymentMethod);

            if (success)
            {
                HttpContext.Session.Remove(CartSessionKey);

                if (paymentMethod == "QR")
                {
                    // Redirect đến trang QR để người dùng quét và thanh toán
                    var totalAmount = cart.Sum(i => i.SubTotal);
                    return RedirectToAction("PayByQR", new { orderId, amount = totalAmount });
                }

                TempData["SuccessMessage"] = message;
                return RedirectToAction("OrderSuccess");
            }

            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }

        // ── GET /Cart/PayByQR ─────────────────────────────────────────────────
        [HttpGet]
        public IActionResult PayByQR(int orderId, decimal amount)
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login", "Account");

            ViewBag.OrderId = orderId;
            ViewBag.Amount = amount;
            ViewBag.BankCode = _config["BankInfo:BankCode"] ?? "MB";
            ViewBag.AccountNo = _config["BankInfo:AccountNo"] ?? "0000000000";
            ViewBag.AccountName = _config["BankInfo:AccountName"] ?? "BOOK STORE";
            return View();
        }

        // ── GET /Cart/CheckPaymentStatus (AJAX polling) ────────────────────────
        [HttpGet]
        public async Task<IActionResult> CheckPaymentStatus(int orderId)
        {
            var (isPaid, status) = await _apiService.GetPaymentStatusAsync(orderId);
            return Json(new { isPaid, status });
        }

        // ── POST /Cart/ExpireOrder (AJAX – gọi khi countdown hết giờ) ────────────
        [HttpPost]
        public async Task<IActionResult> ExpireOrder(int orderId)
        {
            var cancelled = await _apiService.ExpireOrderAsync(orderId);
            return Json(new { cancelled });
        }

        // ── GET /Cart/OrderSuccess ────────────────────────────────────────────
        public IActionResult OrderSuccess()
        {
            return View();
        }
    }
}
