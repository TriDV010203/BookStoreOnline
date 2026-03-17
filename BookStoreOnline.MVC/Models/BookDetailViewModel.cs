namespace BookStoreOnline.MVC.Models
{
    public class BookDetailViewModel
    {
        // Thông tin sách
        public BookViewModel Book { get; set; } = new();

        // Danh sách review
        public List<ReviewViewModel> Reviews { get; set; } = new();

        // Trạng thái review của user hiện tại
        public bool CanReview { get; set; }
        public bool HasReviewed { get; set; }
        public bool HasPurchased { get; set; }

        // Thống kê
        public double AverageRating => Reviews.Count > 0 ? Math.Round(Reviews.Average(r => r.Rating), 1) : 0;
        public int TotalReviews => Reviews.Count;
    }
}
