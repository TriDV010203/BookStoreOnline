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

            await Task.WhenAll(booksTask, categoriesTask, reviewsTask);

            var books      = booksTask.Result;
            var categories = categoriesTask.Result;
            var allReviews = reviewsTask.Result;

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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
