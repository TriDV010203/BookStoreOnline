using BookStoreOnline.MVC.Models;
using BookStoreOnline.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreOnline.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApiService _apiService;

        public AccountController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, message, user) = await _apiService.LoginAsync(model);

            if (success && user != null)
            {
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserFullName", user.FullName);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserRole", user.Role);

                TempData["SuccessMessage"] = $"Chào mừng, {user.FullName}!";

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, message) = await _apiService.RegisterAsync(model);

            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction("Login");
            }

            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Đăng xuất thành công!";
            return RedirectToAction("Index", "Home");
        }

        // ── PROFILE ──────────────────────────────────────────────────────────

        // GET: /Account/Profile
        [HttpGet]
        public IActionResult Profile()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login");

            var model = new UpdateProfileViewModel
            {
                FullName = HttpContext.Session.GetString("UserFullName") ?? "",
                Email = HttpContext.Session.GetString("UserEmail") ?? ""
            };
            return View(model);
        }

        // POST: /Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UpdateProfileViewModel model)
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(model);

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdStr, out int userId))
            {
                TempData["ErrorMessage"] = "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login");
            }

            var (success, message) = await _apiService.UpdateProfileAsync(userId, model);

            if (success)
            {
                // Cập nhật lại session
                HttpContext.Session.SetString("UserFullName", model.FullName);
                HttpContext.Session.SetString("UserEmail", model.Email);
                TempData["SuccessMessage"] = message;
                return RedirectToAction("Profile");
            }

            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }

        // ── CHANGE PASSWORD ───────────────────────────────────────────────────

        // GET: /Account/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login");
            return View();
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(model);

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdStr, out int userId))
            {
                TempData["ErrorMessage"] = "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login");
            }

            var (success, message) = await _apiService.ChangePasswordAsync(userId, model);

            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction("Profile");
            }

            ModelState.AddModelError(string.Empty, message);
            return View(model);
        }
    }
}
