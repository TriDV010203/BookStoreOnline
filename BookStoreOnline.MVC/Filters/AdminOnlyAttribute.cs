using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BookStoreOnline.MVC.Filters
{
    public class AdminOnlyAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var role = session.GetString("UserRole");

            if (string.IsNullOrEmpty(role))
            {
                // Chưa đăng nhập
                context.Result = new RedirectToActionResult("Login", "Account",
                    new { returnUrl = context.HttpContext.Request.Path });
                return;
            }

            if (role != "Admin")
            {
                // Đã đăng nhập nhưng không phải Admin
                context.HttpContext.Session.SetString("AccessDenied", "true");
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
