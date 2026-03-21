using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreOnline.API.Models
{
    /// <summary>
    /// Chương trình giảm giá theo %
    /// </summary>
    public class Discount
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Phần trăm giảm giá (0 - 100)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercent { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Null = vĩnh viễn
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Áp dụng cho tất cả sách
        /// </summary>
        public bool ApplyToAll { get; set; } = false;

        /// <summary>
        /// Danh sách CategoryId, lưu dạng "1,2,3". Null nếu ApplyToAll = true
        /// </summary>
        [MaxLength(500)]
        public string? CategoryIds { get; set; }

        /// <summary>
        /// Nội dung thông báo gửi đến người dùng (optional)
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
