using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreOnline.API.Models
{
    /// <summary>
    /// Thông báo gửi đến người dùng (đặt hàng thành công, khuyến mãi, v.v.)
    /// </summary>
    public class UserNotification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Message { get; set; }

        /// <summary>
        /// Loại thông báo: "Order" hoặc "Discount"
        /// </summary>
        [MaxLength(50)]
        public string Type { get; set; } = "Order";

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
