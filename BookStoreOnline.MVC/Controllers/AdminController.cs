using BookStoreOnline.MVC.Filters;
using BookStoreOnline.MVC.Models;
using BookStoreOnline.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreOnline.MVC.Controllers
{
    [AdminOnly]
    public class AdminController : Controller
    {
        private readonly IApiService _apiService;

        public AdminController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // =====================================================================
        // DASHBOARD
        // =====================================================================

        public async Task<IActionResult> Index()
        {
            var books = await _apiService.GetBooksAsync();
            var categories = await _apiService.GetCategoriesAsync();
            var orders = await _apiService.GetAllOrdersAsync();
            var users = await _apiService.GetAllUsersAsync();

            ViewData["TotalBooks"] = books.Count;
            ViewData["TotalCategories"] = categories.Count;
            ViewData["TotalStock"] = books.Sum(b => b.StockQuantity);
            ViewData["TotalOrders"] = orders.Count;
            ViewData["TotalRevenue"] = orders.Where(o => o.OrderStatus != "Cancelled").Sum(o => o.TotalAmount);
            ViewData["TotalUsers"] = users.Count;
            ViewData["RecentBooks"] = books.Take(5).ToList();

            return View();
        }

        // =====================================================================
        // CATEGORIES
        // =====================================================================

        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var categories = await _apiService.GetCategoriesAsync();
            var books = await _apiService.GetBooksAsync();
            foreach (var cat in categories)
                cat.BookCount = books.Count(b => b.CategoryId == cat.Id);

            return View(categories);
        }

        [HttpGet]
        public IActionResult CategoryCreate()
        {
            return View(new CategoryCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryCreate(CategoryCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, message) = await _apiService.CreateCategoryAsync(model);
            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction("Categories");
            }

            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CategoryEdit(int id)
        {
            var cat = await _apiService.GetCategoryByIdAsync(id);
            if (cat == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục.";
                return RedirectToAction("Categories");
            }

            var model = new CategoryCreateViewModel
            {
                Id = cat.Id,
                Name = cat.Name,
                Description = cat.Description
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryEdit(int id, CategoryCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, message) = await _apiService.UpdateCategoryAsync(id, model);
            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction("Categories");
            }

            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryDelete(int id)
        {
            var (success, message) = await _apiService.DeleteCategoryAsync(id);
            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToAction("Categories");
        }

        // =====================================================================
        // BOOKS
        // =====================================================================

        [HttpGet]
        public async Task<IActionResult> Books()
        {
            var books = await _apiService.GetBooksAsync();
            return View(books);
        }

        [HttpGet]
        public async Task<IActionResult> BookCreate()
        {
            var categories = await _apiService.GetCategoriesAsync();
            var model = new BookCreateViewModel { Categories = categories };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookCreate(BookCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _apiService.GetCategoriesAsync();
                return View(model);
            }

            var (success, message) = await _apiService.CreateBookAsync(model);
            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction("Books");
            }

            ModelState.AddModelError(string.Empty, message);
            model.Categories = await _apiService.GetCategoriesAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> BookEdit(int id)
        {
            var book = await _apiService.GetBookByIdAsync(id);
            if (book == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sách.";
                return RedirectToAction("Books");
            }

            var categories = await _apiService.GetCategoriesAsync();
            var model = new BookCreateViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Description = book.Description,
                Price = book.Price,
                StockQuantity = book.StockQuantity,
                ImageUrl = book.ImageUrl,
                CategoryId = book.CategoryId,
                Categories = categories
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookEdit(int id, BookCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _apiService.GetCategoriesAsync();
                return View(model);
            }

            var (success, message) = await _apiService.UpdateBookAsync(id, model);
            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction("Books");
            }

            ModelState.AddModelError(string.Empty, message);
            model.Categories = await _apiService.GetCategoriesAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookDelete(int id)
        {
            var (success, message) = await _apiService.DeleteBookAsync(id);
            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToAction("Books");
        }

        // =====================================================================
        // ORDERS
        // =====================================================================

        [HttpGet]
        public async Task<IActionResult> Orders(string? status)
        {
            var orders = await _apiService.GetAllOrdersAsync();
            if (!string.IsNullOrEmpty(status))
                orders = orders.Where(o => o.OrderStatus == status).ToList();

            ViewData["StatusFilter"] = status ?? "";
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetail(int id)
        {
            var order = await _apiService.GetOrderDetailAsync(id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Orders");
            }
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var (success, message) = await _apiService.UpdateOrderStatusAsync(id, status);
            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToAction("OrderDetail", new { id });
        }

        // =====================================================================
        // USERS
        // =====================================================================

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _apiService.GetAllUsersAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUser(int id)
        {
            // Không thể tự ban chính mình
            var selfIdStr = HttpContext.Session.GetString("UserId");
            if (int.TryParse(selfIdStr, out int selfId) && selfId == id)
            {
                TempData["ErrorMessage"] = "Bạn không thể khóa tài khoản của chính mình!";
                return RedirectToAction("Users");
            }

            var (success, message) = await _apiService.BanUserAsync(id);
            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnbanUser(int id)
        {
            var (success, message) = await _apiService.UnbanUserAsync(id);
            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToAction("Users");
        }

        // =====================================================================
        // DISCOUNTS
        // =====================================================================

        [HttpGet]
        public async Task<IActionResult> Discounts()
        {
            var discounts = await _apiService.GetDiscountsAsync();
            return View(discounts);
        }

        [HttpGet]
        public async Task<IActionResult> DiscountCreate()
        {
            var categories = await _apiService.GetCategoriesAsync();
            return View(new DiscountCreateViewModel { Categories = categories });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DiscountCreate(DiscountCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _apiService.GetCategoriesAsync();
                return View(model);
            }

            var (success, message) = await _apiService.CreateDiscountAsync(model);
            if (success)
            {
                // Broadcast notification to all users if discount has description
                if (!string.IsNullOrWhiteSpace(model.Description))
                    await _apiService.BroadcastDiscountNotificationAsync($"🎉 Khuyến mãi mới: {model.Name}", model.Description);
                else if (model.IsActive)
                    await _apiService.BroadcastDiscountNotificationAsync($"🎉 Khuyến mãi mới: {model.Name}", $"Giảm {model.DiscountPercent}% - Áp dụng ngay hôm nay!");

                TempData["SuccessMessage"] = message;
                return RedirectToAction("Discounts");
            }

            ModelState.AddModelError(string.Empty, message);
            model.Categories = await _apiService.GetCategoriesAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DiscountEdit(int id)
        {
            var discount = await _apiService.GetDiscountByIdAsync(id);
            if (discount == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy chương trình giảm giá.";
                return RedirectToAction("Discounts");
            }

            var categories = await _apiService.GetCategoriesAsync();
            var selectedCatIds = new List<int>();
            if (!string.IsNullOrEmpty(discount.CategoryIds))
                selectedCatIds = discount.CategoryIds.Split(',').Select(s => int.TryParse(s.Trim(), out int val) ? val : 0).Where(v => v > 0).ToList();

            var model = new DiscountCreateViewModel
            {
                Id = discount.Id,
                Name = discount.Name,
                DiscountPercent = discount.DiscountPercent,
                StartDate = discount.StartDate,
                EndDate = discount.EndDate,
                ApplyToAll = discount.ApplyToAll,
                SelectedCategoryIds = selectedCatIds,
                Description = discount.Description,
                IsActive = discount.IsActive,
                Categories = categories
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DiscountEdit(int id, DiscountCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _apiService.GetCategoriesAsync();
                return View(model);
            }

            var (success, message) = await _apiService.UpdateDiscountAsync(id, model);
            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction("Discounts");
            }

            ModelState.AddModelError(string.Empty, message);
            model.Categories = await _apiService.GetCategoriesAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DiscountDelete(int id)
        {
            var (success, message) = await _apiService.DeleteDiscountAsync(id);
            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToAction("Discounts");
        }

        // =====================================================================
        // AJAX – REALTIME SUPPORT
        // =====================================================================

        [HttpGet]
        public async Task<IActionResult> GetPendingOrderCount()
        {
            var orders = await _apiService.GetAllOrdersAsync();
            var count = orders.Count(o => o.OrderStatus == "Pending");
            return Json(new { count });
        }
    }
}
