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
    }
}
