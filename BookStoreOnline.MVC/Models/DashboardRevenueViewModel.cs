namespace BookStoreOnline.MVC.Models
{
    public class DashboardRevenueViewModel
    {
        public int Year { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public int TotalOrders { get; set; }
        public List<MonthlyRevenueItem> MonthlyRevenue { get; set; } = new();
        public List<CategoryStatItem> CategoryStats { get; set; } = new();
    }

    public class MonthlyRevenueItem
    {
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
    }

    public class CategoryStatItem
    {
        public string CategoryName { get; set; } = string.Empty;
        public int SoldQty { get; set; }
        public decimal Revenue { get; set; }
    }
}
