namespace BookStoreOnline.MVC.Models
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = "Pending";
        public string ShippingAddress { get; set; } = "";
        public string PaymentMethod { get; set; } = "COD";
        public int UserId { get; set; }
        public string? UserFullName { get; set; }
        public string? UserEmail { get; set; }
        public List<OrderDetailViewModel> OrderDetails { get; set; } = new();
    }

    public class OrderDetailViewModel
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = "";
        public string? BookAuthor { get; set; }
        public string? BookImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal => Quantity * UnitPrice;
    }

    public class UserAdminViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "Customer";
        public bool IsBanned { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class HomeIndexViewModel
    {
        public List<BookViewModel> Books { get; set; } = new();
        public List<CategoryViewModel> Categories { get; set; } = new();
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public string? PriceSort { get; set; } // "asc" or "desc"
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
