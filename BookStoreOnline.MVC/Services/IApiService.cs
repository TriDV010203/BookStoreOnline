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

        // ===== USER PROFILE =====
        Task<(bool success, string message)> UpdateProfileAsync(int id, UpdateProfileViewModel model);
        Task<(bool success, string message)> ChangePasswordAsync(int id, ChangePasswordViewModel model);

        // ===== ORDERS =====
        Task<(bool success, string message, int orderId)> PlaceOrderAsync(int userId, string shippingAddress, List<CartItemViewModel> items, string paymentMethod = "COD");
        Task<List<OrderViewModel>> GetOrdersByUserAsync(int userId);
        Task<List<OrderViewModel>> GetAllOrdersAsync();
        Task<OrderViewModel?> GetOrderDetailAsync(int orderId);
        Task<(bool success, string message)> UpdateOrderStatusAsync(int orderId, string status);
        Task<(bool isPaid, string status)> GetPaymentStatusAsync(int orderId);
        Task<bool> ExpireOrderAsync(int orderId);

        // ===== USERS (ADMIN) =====
        Task<List<UserAdminViewModel>> GetAllUsersAsync();
        Task<(bool success, string message)> BanUserAsync(int userId);
        Task<(bool success, string message)> UnbanUserAsync(int userId);

        // ===== REVIEWS =====
        Task<List<ReviewViewModel>> GetReviewsByBookAsync(int bookId);
        Task<List<ReviewViewModel>> GetAllReviewsAsync();
        Task<(bool canReview, bool hasPurchased, bool hasReviewed)> CanReviewAsync(int userId, int bookId);
        Task<(bool success, string message)> CreateReviewAsync(CreateReviewViewModel model);
    }
}
