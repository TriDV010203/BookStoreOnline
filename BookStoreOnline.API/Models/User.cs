using System.ComponentModel.DataAnnotations;

namespace BookStoreOnline.API.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [MaxLength(20)]
        public string Role { get; set; } = "Customer"; // Mặc định là Customer, Admin sẽ set tay

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}