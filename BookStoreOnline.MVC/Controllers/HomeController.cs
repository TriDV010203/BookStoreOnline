using BookStoreOnline.MVC.Models;
using BookStoreOnline.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookStoreOnline.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IApiService _apiService;

        public HomeController(ILogger<HomeController> logger, IApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        public async Task<IActionResult> Index(
            string? search,
            int? categoryId,
            string? priceSort,
            int page = 1)
        {
            const int pageSize = 12;

            var booksTask    = _apiService.GetBooksAsync();
            var categoriesTask = _apiService.GetCategoriesAsync();
            var reviewsTask  = _apiService.GetAllReviewsAsync();
            var discountsTask = _apiService.GetDiscountsAsync();

            await Task.WhenAll(booksTask, categoriesTask, reviewsTask, discountsTask);

            var books      = booksTask.Result;
            var categories = categoriesTask.Result;
            var allReviews = reviewsTask.Result;
            var discounts  = discountsTask.Result;

            // Build active discounts lookup per categoryId
            var now = DateTime.Now;
            var activeDiscounts = discounts.Where(d =>
                d.IsActive && d.StartDate <= now && (d.EndDate == null || d.EndDate >= now)).ToList();

            // Attach per-book rating stats
            var ratingByBook = allReviews
                .GroupBy(r => r.BookId)
                .ToDictionary(g => g.Key, g => (avg: Math.Round(g.Average(r => r.Rating), 1), count: g.Count()));

            foreach (var b in books)
            {
                if (ratingByBook.TryGetValue(b.Id, out var stat))
                {
                    b.AverageRating = stat.avg;
                    b.ReviewCount   = stat.count;
                }
                // Apply best discount
                decimal best = 0;
                foreach (var d in activeDiscounts)
                {
                    if (d.ApplyToAll && d.DiscountPercent > best) best = d.DiscountPercent;
                    else if (!d.ApplyToAll && !string.IsNullOrEmpty(d.CategoryIds))
                    {
                        var ids = d.CategoryIds.Split(',').Select(s => int.TryParse(s.Trim(), out int v) ? v : 0);
                        if (ids.Contains(b.CategoryId) && d.DiscountPercent > best) best = d.DiscountPercent;
                    }
                }
                b.DiscountPercent = best;
            }

            // Filter by keyword
            if (!string.IsNullOrWhiteSpace(search))
            {
                books = books.Where(b =>
                    b.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    b.Author.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Filter by category
            if (categoryId.HasValue && categoryId.Value > 0)
                books = books.Where(b => b.CategoryId == categoryId.Value).ToList();

            // Sort by price
            if (priceSort == "asc")
                books = books.OrderBy(b => b.Price).ToList();
            else if (priceSort == "desc")
                books = books.OrderByDescending(b => b.Price).ToList();

            var totalCount = books.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var pagedBooks = books.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var vm = new HomeIndexViewModel
            {
                Books      = pagedBooks,
                Categories = categories,
                Search     = search,
                CategoryId = categoryId,
                PriceSort  = priceSort,
                Page       = page,
                PageSize   = pageSize,
                TotalCount = totalCount
            };

            return View(vm);
        }

        // GET: /Home/Detail/{id}
        public async Task<IActionResult> Detail(int id)
        {
            var book = await _apiService.GetBookByIdAsync(id);
            if (book == null) return NotFound();

            // Load discount for this book and apply
            var discountPercent = await _apiService.GetDiscountPercentForBookAsync(id);
            book.DiscountPercent = discountPercent;

            var reviews = await _apiService.GetReviewsByBookAsync(id);

            var vm = new BookDetailViewModel
            {
                Book = book,
                Reviews = reviews
            };

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (int.TryParse(userIdStr, out int userId))
            {
                var (canReview, hasPurchased, hasReviewed) = await _apiService.CanReviewAsync(userId, id);
                vm.CanReview   = canReview;
                vm.HasPurchased = hasPurchased;
                vm.HasReviewed  = hasReviewed;
            }

            return View(vm);
        }

        // POST: /Home/Detail/{id}  — submit a review
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Detail(int id, CreateReviewViewModel model)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdStr, out int userId))
                return RedirectToAction("Login", "Account", new { returnUrl = $"/Home/Detail/{id}" });

            model.BookId = id;
            model.UserId = userId;

            if (!ModelState.IsValid)
            {
                TempData["ReviewError"] = "Vui lòng chọn số sao và nhập bình luận hợp lệ.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            var (success, message) = await _apiService.CreateReviewAsync(model);
            TempData[success ? "ReviewSuccess" : "ReviewError"] = message;
            return RedirectToAction(nameof(Detail), new { id });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // ===== AJAX: User Bell Notifications =====

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdStr, out int userId))
                return Json(new { notifications = new object[0], unread = 0 });

            var notifications = await _apiService.GetUserNotificationsAsync(userId);
            var unread = notifications.Count(n => !n.IsRead);
            return Json(new { notifications, unread });
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationRead(int id)
        {
            await _apiService.MarkNotificationReadAsync(id);
            return Json(new { ok = true });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllNotificationsRead()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (int.TryParse(userIdStr, out int userId))
                await _apiService.MarkAllNotificationsReadAsync(userId);
            return Json(new { ok = true });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
