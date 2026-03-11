using BookStoreOnline.MVC.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace BookStoreOnline.MVC.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<BookViewModel>> GetBooksAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/books");
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var books = await response.Content.ReadFromJsonAsync<List<BookViewModel>>(options);
                    if (books != null)
                    {
                        // Map category name nếu có
                        foreach (var book in books)
                        {
                            if (book.CategoryName == null)
                                book.CategoryName = "Chưa phân loại";
                        }
                        return books;
                    }
                }
                return new List<BookViewModel>();
            }
            catch
            {
                return new List<BookViewModel>();
            }
        }

        public async Task<(bool success, string message, UserSessionData? user)> LoginAsync(LoginViewModel model)
        {
            try
            {
                var payload = new
                {
                    email = model.Email,
                    password = model.Password
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("api/users/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var userData = await response.Content.ReadFromJsonAsync<UserSessionData>(options);
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
            catch (Exception ex)
            {
                return (false, $"Không thể kết nối đến máy chủ: {ex.Message}", null);
            }
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

                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("api/users/register", content);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Đăng ký thành công! Vui lòng đăng nhập.");
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    return (false, errorMsg.Trim('"'));
                }
            }
            catch (Exception ex)
            {
                return (false, $"Không thể kết nối đến máy chủ: {ex.Message}");
            }
        }
    }
}
