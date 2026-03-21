namespace BookStoreOnline.MVC.Models
{
    public class UserNotificationViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string Type { get; set; } = "Order";
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
