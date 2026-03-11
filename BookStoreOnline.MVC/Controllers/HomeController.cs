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

        public async Task<IActionResult> Index(string? search, int? categoryId)
        {
            var books = await _apiService.GetBooksAsync();

            // Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                books = books.Where(b =>
                    b.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    b.Author.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
                ViewData["Search"] = search;
            }

            // Lọc theo danh mục
            if (categoryId.HasValue)
            {
                books = books.Where(b => b.CategoryId == categoryId.Value).ToList();
                ViewData["CategoryId"] = categoryId;
            }

            return View(books);
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
