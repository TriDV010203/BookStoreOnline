using BookStoreOnline.MVC.Models;

namespace BookStoreOnline.MVC.Services
{
    public interface IApiService
    {
        // ===== BOOKS =====
        Task<List<BookViewModel>> GetBooksAsync();
        Task<BookViewModel?> GetBookByIdAsync(int id);
        Task<(bool success, string message)> CreateBookAsync(BookCreateViewModel model);
        Task<(bool success, string message)> UpdateBookAsync(int id, BookCreateViewModel model);
        Task<(bool success, string message)> DeleteBookAsync(int id);

        // ===== CATEGORIES =====
        Task<List<CategoryViewModel>> GetCategoriesAsync();
        Task<CategoryViewModel?> GetCategoryByIdAsync(int id);
        Task<(bool success, string message)> CreateCategoryAsync(CategoryCreateViewModel model);
        Task<(bool success, string message)> UpdateCategoryAsync(int id, CategoryCreateViewModel model);
        Task<(bool success, string message)> DeleteCategoryAsync(int id);

        // ===== AUTH =====
        Task<(bool success, string message, UserSessionData? user)> LoginAsync(LoginViewModel model);
        Task<(bool success, string message)> RegisterAsync(RegisterViewModel model);
    }
}
