namespace BookStoreOnline.MVC.Models
{
    public class ReviewViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int BookId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
