using BookStoreOnline.MVC.Models;

namespace BookStoreOnline.MVC.Services
{
    public interface IApiService
    {
        Task<List<BookViewModel>> GetBooksAsync();
        Task<(bool success, string message, UserSessionData? user)> LoginAsync(LoginViewModel model);
        Task<(bool success, string message)> RegisterAsync(RegisterViewModel model);
    }
}
