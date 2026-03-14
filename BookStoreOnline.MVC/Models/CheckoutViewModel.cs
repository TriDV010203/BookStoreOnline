using System.ComponentModel.DataAnnotations;

namespace BookStoreOnline.MVC.Models
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng.")]
        [StringLength(500, ErrorMessage = "Địa chỉ quá dài.")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; } = string.Empty;

        // Readonly — hiển thị để xác nhận
        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal TotalAmount => Items.Sum(i => i.SubTotal);
    }
}
