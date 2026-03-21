using Microsoft.EntityFrameworkCore;

namespace BookStoreOnline.API.Models // Sửa namespace này nếu bạn bỏ file vào thư mục Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor bắt buộc để nhận cấu hình chuỗi kết nối từ file appsettings.json
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Khai báo các bảng sẽ được tạo trong SQL Server
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        // Cấu hình thêm các hành vi của Database (nếu cần)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Đảm bảo Email của User là duy nhất, không được đăng ký trùng
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Mỗi user chỉ được đánh giá 1 lần cho mỗi sách
            modelBuilder.Entity<ProductReview>()
                .HasIndex(r => new { r.UserId, r.BookId })
                .IsUnique();
        }
    }
}