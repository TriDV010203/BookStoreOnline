using System.ComponentModel.DataAnnotations;

namespace BookStoreOnline.MVC.Models
{
    public class CreateReviewViewModel
    {
        [Required]
        public int BookId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Vui lòng chọn từ 1 đến 5 sao.")]
        public int Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Bình luận tối đa 1000 ký tự.")]
        public string? Comment { get; set; }
    }
}
