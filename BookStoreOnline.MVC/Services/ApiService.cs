using BookStoreOnline.MVC.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace BookStoreOnline.MVC.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // =====================================================================
        // BOOKS
        // =====================================================================

        public async Task<List<BookViewModel>> GetBooksAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/books");
                if (response.IsSuccessStatusCode)
                {
                    var books = await response.Content.ReadFromJsonAsync<List<BookViewModel>>(_jsonOptions);
                    if (books != null)
                    {
                        foreach (var book in books)
                            if (book.CategoryName == null)
                                book.CategoryName = "Chưa phân loại";
                        return books;
                    }
                }
                return new List<BookViewModel>();
            }
            catch { return new List<BookViewModel>(); }
        }

        public async Task<BookViewModel?> GetBookByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/books/{id}");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<BookViewModel>(_jsonOptions);
                return null;
            }
            catch { return null; }
        }

        public async Task<(bool success, string message)> CreateBookAsync(BookCreateViewModel model)
        {
            try
            {
                var payload = new
                {
                    title = model.Title,
                    author = model.Author,
                    description = model.Description,
                    price = model.Price,
                    stockQuantity = model.StockQuantity,
                    imageUrl = model.ImageUrl,
                    categoryId = model.CategoryId
                };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/books", content);
                if (response.IsSuccessStatusCode)
                    return (true, "Thêm sách thành công!");
                var err = await response.Content.ReadAsStringAsync();
                return (false, err.Trim('"'));
            }
            catch (Exception ex) { return (false, $"Lỗi kết nối: {ex.Message}"); }
        }

        public async Task<(bool success, string message)> UpdateBookAsync(int id, BookCreateViewModel model)
        {
            try
            {
                var payload = new
                {
                    id,
                    title = model.Title,
                    author = model.Author,
                    description = model.Description,
                    price = model.Price,
                    stockQuantity = model.StockQuantity,
                    imageUrl = model.ImageUrl,
                    categoryId = model.CategoryId
                };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"api/books/{id}", content);
                if (response.IsSuccessStatusCode)
                    return (true, "Cập nhật sách thành công!");
                var err = await response.Content.ReadAsStringAsync();
                return (false, err.Trim('"'));
            }
            catch (Exception ex) { return (false, $"Lỗi kết nối: {ex.Message}"); }
        }

        public async Task<(bool success, string message)> DeleteBookAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/books/{id}");
                if (response.IsSuccessStatusCode)
                    return (true, "Xóa sách thành công!");
                return (false, "Không thể xóa sách.");
            }
            catch (Exception ex) { return (false, $"Lỗi kết nối: {ex.Message}"); }
        }

        // =====================================================================
        // CATEGORIES
        // =====================================================================

        public async Task<List<CategoryViewModel>> GetCategoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/categories");
                if (response.IsSuccessStatusCode)
                {
                    var cats = await response.Content.ReadFromJsonAsync<List<CategoryViewModel>>(_jsonOptions);
                    return cats ?? new List<CategoryViewModel>();
                }
                return new List<CategoryViewModel>();
            }
            catch { return new List<CategoryViewModel>(); }
        }

        public async Task<CategoryViewModel?> GetCategoryByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/categories/{id}");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<CategoryViewModel>(_jsonOptions);
                return null;
            }
            catch { return null; }
        }

        public async Task<(bool success, string message)> CreateCategoryAsync(CategoryCreateViewModel model)
        {
            try
            {
                var payload = new { name = model.Name, description = model.Description };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/categories", content);
                if (response.IsSuccessStatusCode)
                    return (true, "Thêm danh mục thành công!");
                var err = await response.Content.ReadAsStringAsync();
                return (false, err.Trim('"'));
            }
            catch (Exception ex) { return (false, $"Lỗi kết nối: {ex.Message}"); }
        }

        public async Task<(bool success, string message)> UpdateCategoryAsync(int id, CategoryCreateViewModel model)
        {
            try
            {
                var payload = new { id, name = model.Name, description = model.Description };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"api/categories/{id}", content);
                if (response.IsSuccessStatusCode)
                    return (true, "Cập nhật danh mục thành công!");
                var err = await response.Content.ReadAsStringAsync();
                return (false, err.Trim('"'));
            }
            catch (Exception ex) { return (false, $"Lỗi kết nối: {ex.Message}"); }
        }

        public async Task<(bool success, string message)> DeleteCategoryAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/categories/{id}");
                if (response.IsSuccessStatusCode)
                    return (true, "Xóa danh mục thành công!");
                return (false, "Không thể xóa danh mục (có thể đang có sách thuộc danh mục này).");
            }
            catch (Exception ex) { return (false, $"Lỗi kết nối: {ex.Message}"); }
        }

        // =====================================================================
        // AUTH
        // =====================================================================

        public async Task<(bool success, string message, UserSessionData? user)> LoginAsync(LoginViewModel model)
        {
            try
            {
                var payload = new { email = model.Email, password = model.Password };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/users/login", content);
                if (response.IsSuccessStatusCode)
                {
                    var userData = await response.Content.ReadFromJsonAsync<UserSessionData>(_jsonOptions);
                    if (userData != null)
                        return (true, "Đăng nhập thành công!", userData);
                    return (false, "Lỗi xử lý dữ liệu.", null);
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    return (false, errorMsg.Trim('"'), null);
                }
            }
            catch (Exception ex) { return (false, $"Không thể kết nối đến máy chủ: {ex.Message}", null); }
        }

        public async Task<(bool success, string message)> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                var payload = new
                {
                    fullName = model.FullName,
                    email = model.Email,
                    passwordHash = model.Password,
                    role = "Customer"
                };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/users/register", content);
                if (response.IsSuccessStatusCode)
                    return (true, "Đăng ký thành công! Vui lòng đăng nhập.");
                var errorMsg = await response.Content.ReadAsStringAsync();
                return (false, errorMsg.Trim('"'));
            }
            catch (Exception ex) { return (false, $"Không thể kết nối đến máy chủ: {ex.Message}"); }
        }

        // =====================================================================
        // USER PROFILE
        // =====================================================================

        public async Task<(bool success, string message)> UpdateProfileAsync(int id, UpdateProfileViewModel model)
        {
            try
            {
                var payload = new { fullName = model.FullName, email = model.Email };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"api/users/{id}", content);
                if (response.IsSuccessStatusCode)
                    return (true, "Cập nhật thông tin thành công!");
                var err = await response.Content.ReadAsStringAsync();
                return (false, err.Trim('"'));
            }
            catch (Exception ex) { return (false, $"Lỗi kết nối: {ex.Message}"); }
        }

        public async Task<(bool success, string message)> ChangePasswordAsync(int id, ChangePasswordViewModel model)
        {
            try
            {
                var payload = new { currentPassword = model.CurrentPassword, newPassword = model.NewPassword };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"api/users/{id}/change-password", content);
                if (response.IsSuccessStatusCode)
                    return (true, "Đổi mật khẩu thành công!");
                var err = await response.Content.ReadAsStringAsync();
                return (false, err.Trim('"'));
            }
            catch (Exception ex) { return (false, $"Lỗi kết nối: {ex.Message}"); }
        }

        // =====================================================================
        // ORDERS
        // =====================================================================

        public async Task<(bool success, string message)> PlaceOrderAsync(int userId, string shippingAddress, List<CartItemViewModel> items)
        {
            try
            {
                var payload = new
                {
                    userId,
                    totalAmount = items.Sum(i => i.SubTotal),
                    shippingAddress,
                    orderStatus = "Pending",
                    orderDetails = items.Select(i => new
                    {
                        bookId = i.BookId,
                        quantity = i.Quantity,
                        unitPrice = i.UnitPrice
                    }).ToList()
                };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/orders", content);
                if (response.IsSuccessStatusCode)
                    return (true, "Đặt hàng thành công! Cảm ơn bạn đã mua hàng.");
                var err = await response.Content.ReadAsStringAsync();
                return (false, err.Trim('"'));
            }
            catch (Exception ex) { return (false, $"Lỗi kết nối: {ex.Message}"); }
        }

        public async Task<List<OrderViewModel>> GetOrdersByUserAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/orders/user/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<ApiOrderDto>>(_jsonOptions);
                    return data?.Select(MapToOrderViewModel).ToList() ?? new();
                }
                return new();
            }
            catch { return new(); }
        }

        public async Task<List<OrderViewModel>> GetAllOrdersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/orders");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<ApiOrderDto>>(_jsonOptions);
                    return data?.Select(MapToOrderViewModel).ToList() ?? new();
                }
                return new();
            }
            catch { return new(); }
        }

        public async Task<OrderViewModel?> GetOrderDetailAsync(int orderId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/orders/{orderId}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<ApiOrderDto>(_jsonOptions);
                    return data != null ? MapToOrderViewModel(data) : null;
                }
                return null;
            }
            catch { return null; }
        }

        public async Task<(bool success, string message)> UpdateOrderStatusAsync(int orderId, string status)
        {
            try
            {
                var payload = new { status };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync($"api/orders/{orderId}/status", content);
                if (response.IsSuccessStatusCode)
                    return (true, "Cập nhật trạng thái thành công!");
                var err = await response.Content.ReadAsStringAsync();
                return (false, err.Trim('"'));
            }
            catch (Exception ex) { return (false, $"Lỗi kết nối: {ex.Message}"); }
        }

        // =====================================================================
        // USERS (ADMIN)
        // =====================================================================

        public async Task<List<UserAdminViewModel>> GetAllUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/users");
                if (response.IsSuccessStatusCode)
                {
                    var users = await response.Content.ReadFromJsonAsync<List<UserAdminViewModel>>(_jsonOptions);
                    return users ?? new();
                }
                return new();
            }
            catch { return new(); }
        }

        public async Task<(bool success, string message)> BanUserAsync(int userId)
        {
            try
            {
                var content = new StringContent("{}", Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync($"api/users/{userId}/ban", content);
                if (response.IsSuccessStatusCode)
                    return (true, "Đã khóa tài khoản thành công.");
                var err = await response.Content.ReadAsStringAsync();
                return (false, err.Trim('"'));
            }
            catch (Exception ex) { return (false, $"Lỗi kết nối: {ex.Message}"); }
        }

        public async Task<(bool success, string message)> UnbanUserAsync(int userId)
        {
            try
            {
                var content = new StringContent("{}", Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync($"api/users/{userId}/unban", content);
                if (response.IsSuccessStatusCode)
                    return (true, "Đã mở khóa tài khoản thành công.");
                var err = await response.Content.ReadAsStringAsync();
                return (false, err.Trim('"'));
            }
            catch (Exception ex) { return (false, $"Lỗi kết nối: {ex.Message}"); }
        }

        // =====================================================================
        // REVIEWS
        // =====================================================================

        public async Task<List<ReviewViewModel>> GetReviewsByBookAsync(int bookId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/reviews/book/{bookId}");
                if (response.IsSuccessStatusCode)
                {
                    var reviews = await response.Content.ReadFromJsonAsync<List<ReviewViewModel>>(_jsonOptions);
                    return reviews ?? new();
                }
                return new();
            }
            catch { return new(); }
        }

        public async Task<List<ReviewViewModel>> GetAllReviewsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/reviews");
                if (response.IsSuccessStatusCode)
                {
                    var reviews = await response.Content.ReadFromJsonAsync<List<ReviewViewModel>>(_jsonOptions);
                    return reviews ?? new();
                }
                return new();
            }
            catch { return new(); }
        }

        public async Task<(bool canReview, bool hasPurchased, bool hasReviewed)> CanReviewAsync(int userId, int bookId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/reviews/can-review?userId={userId}&bookId={bookId}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CanReviewResponse>(_jsonOptions);
                    if (result != null)
                        return (result.CanReview, result.HasPurchased, result.HasReviewed);
                }
                return (false, false, false);
            }
            catch { return (false, false, false); }
        }

        public async Task<(bool success, string message)> CreateReviewAsync(CreateReviewViewModel model)
        {
            try
            {
                var payload = new
                {
                    userId = model.UserId,
                    bookId = model.BookId,
                    rating = model.Rating,
                    comment = model.Comment
                };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/reviews", content);
                if (response.IsSuccessStatusCode)
                    return (true, "Đánh giá của bạn đã được gửi thành công!");
                var err = await response.Content.ReadAsStringAsync();
                return (false, err.Trim('"'));
            }
            catch (Exception ex) { return (false, $"Lỗi kết nối: {ex.Message}"); }
        }

        // =====================================================================
        // HELPERS
        // =====================================================================

        private static OrderViewModel MapToOrderViewModel(ApiOrderDto dto)
        {
            return new OrderViewModel
            {
                Id = dto.Id,
                OrderDate = dto.OrderDate,
                TotalAmount = dto.TotalAmount,
                OrderStatus = dto.OrderStatus,
                ShippingAddress = dto.ShippingAddress,
                UserId = dto.UserId,
                UserFullName = dto.User?.FullName,
                UserEmail = dto.User?.Email,
                OrderDetails = dto.OrderDetails?.Select(d => new OrderDetailViewModel
                {
                    Id = d.Id,
                    BookId = d.BookId,
                    BookTitle = d.Book?.Title ?? $"Sách #{d.BookId}",
                    BookAuthor = d.Book?.Author,
                    BookImageUrl = d.Book?.ImageUrl,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice
                }).ToList() ?? new()
            };
        }

        // DTO classes for deserializing API responses
        private class ApiOrderDto
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public ApiUserDto? User { get; set; }
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string OrderStatus { get; set; } = "";
            public string ShippingAddress { get; set; } = "";
            public List<ApiOrderDetailDto>? OrderDetails { get; set; }
        }

        private class ApiOrderDetailDto
        {
            public int Id { get; set; }
            public int BookId { get; set; }
            public ApiBookDto? Book { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }

        private class ApiUserDto
        {
            public int Id { get; set; }
            public string FullName { get; set; } = "";
            public string Email { get; set; } = "";
        }

        private class ApiBookDto
        {
            public int Id { get; set; }
            public string Title { get; set; } = "";
            public string? Author { get; set; }
            public string? ImageUrl { get; set; }
        }

        private class CanReviewResponse
        {
            public bool CanReview { get; set; }
            public bool HasPurchased { get; set; }
            public bool HasReviewed { get; set; }
        }
    }
}
