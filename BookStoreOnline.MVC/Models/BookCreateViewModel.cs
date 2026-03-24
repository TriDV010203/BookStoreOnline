using System.ComponentModel.DataAnnotations;

namespace BookStoreOnline.MVC.Models
{
    public class BookCreateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề sách không được để trống")]
        [MaxLength(200, ErrorMessage = "Tiêu đề tối đa 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên tác giả không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên tác giả tối đa 100 ký tự")]
        [Display(Name = "Tác giả")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả không được để trống")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá tiền không được để trống")]
        [Range(0, 99999999, ErrorMessage = "Giá phải lớn hơn 0")]
        [Display(Name = "Giá Bán (VNĐ)")]
        public decimal Price { get; set; }

        [Range(0, 99999999, ErrorMessage = "Giá nhập không hợp lệ")]
        [Display(Name = "Giá Nhập (VNĐ)")]
        public decimal ImportPrice { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm")]
        [Display(Name = "Số lượng tồn kho")]
        public int StockQuantity { get; set; }

        [Display(Name = "URL hình ảnh")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        // For the dropdown list
        public List<CategoryViewModel> Categories { get; set; } = new();
    }
}
