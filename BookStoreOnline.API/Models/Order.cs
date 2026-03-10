using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreOnline.API.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        // Khóa ngoại liên kết với User
        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(50)]
        public string OrderStatus { get; set; } = "Pending"; // Pending, Processing, Shipped, Delivered, Cancelled

        [Required]
        [MaxLength(500)]
        public string ShippingAddress { get; set; }

        // Navigation property: Một đơn hàng có nhiều chi tiết đơn hàng
        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}