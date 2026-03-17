using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreOnline.API.Models
{
    public class ProductReview
    {
        [Key]
        public int Id { get; set; }

        // FK → User
        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        // FK → Book
        [Required]
        public int BookId { get; set; }
        [ForeignKey("BookId")]
        public Book? Book { get; set; }

        // Rating: 1 → 5 sao
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
