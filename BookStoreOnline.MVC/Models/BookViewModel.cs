namespace BookStoreOnline.MVC.Models
{
    public class BookViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        // Populated from reviews
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }

        // Discount
        public decimal DiscountPercent { get; set; } = 0;
        public decimal? DiscountedPrice => DiscountPercent > 0
            ? Math.Round(Price * (1 - DiscountPercent / 100), 0)
            : null;
    }
}
