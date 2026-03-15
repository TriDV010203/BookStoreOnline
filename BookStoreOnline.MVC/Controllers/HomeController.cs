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

            var books = await _apiService.GetBooksAsync();
            var categories = await _apiService.GetCategoriesAsync();

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
                Books = pagedBooks,
                Categories = categories,
                Search = search,
                CategoryId = categoryId,
                PriceSort = priceSort,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return View(vm);
        }

        // GET: /Home/Detail/{id}
        public async Task<IActionResult> Detail(int id)
        {
            var book = await _apiService.GetBookByIdAsync(id);
            if (book == null)
                return NotFound();
            return View(book);
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
