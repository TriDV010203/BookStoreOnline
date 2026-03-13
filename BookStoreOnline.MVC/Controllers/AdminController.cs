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

            ViewData["TotalBooks"] = books.Count;
            ViewData["TotalCategories"] = categories.Count;
            ViewData["TotalStock"] = books.Sum(b => b.StockQuantity);
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
            // Lấy số sách mỗi category
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
    }
}
