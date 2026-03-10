using System.ComponentModel.DataAnnotations;

namespace BookStoreOnline.API.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; } // Dấu ? nghĩa là cho phép null (không bắt buộc nhập)

        // Navigation property: Một danh mục có nhiều sách
        public ICollection<Book>? Books { get; set; }
    }
}