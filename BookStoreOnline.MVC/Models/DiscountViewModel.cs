using System.ComponentModel.DataAnnotations;

namespace BookStoreOnline.MVC.Models
{
    public class DiscountViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool ApplyToAll { get; set; }
        public string? CategoryIds { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Helper
        public bool IsExpired => EndDate.HasValue && EndDate.Value.ToUniversalTime() < DateTime.UtcNow;
        public bool IsCurrentlyActive => IsActive && !IsExpired && StartDate.ToUniversalTime() <= DateTime.UtcNow;
    }

    public class DiscountCreateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên chương trình giảm giá.")]
        [MaxLength(200)]
        [Display(Name = "Tên chương trình")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100, ErrorMessage = "Phần trăm giảm giá phải từ 0.01 đến 100.")]
        [Display(Name = "Phần trăm giảm (%)")]
        public decimal DiscountPercent { get; set; }

        [Required]
        [Display(Name = "Ngày bắt đầu")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Display(Name = "Ngày kết thúc (để trống = vĩnh viễn)")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Áp dụng cho tất cả sách")]
        public bool ApplyToAll { get; set; } = false;

        /// <summary>
        /// Danh sách CategoryId được chọn
        /// </summary>
        public List<int> SelectedCategoryIds { get; set; } = new();

        [MaxLength(500)]
        [Display(Name = "Nội dung thông báo (optional)")]
        public string? Description { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;

        // For the form dropdown
        public List<CategoryViewModel> Categories { get; set; } = new();
    }
}
