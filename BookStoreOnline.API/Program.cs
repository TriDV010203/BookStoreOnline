
using System.Text.Json.Serialization;
using BookStoreOnline.API.Models;
using Microsoft.EntityFrameworkCore;
namespace BookStoreOnline.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Nhớ thêm using thư viện này ở đầu file nhé:
            // using Microsoft.EntityFrameworkCore;
            // using BookStoreOnline.API.Models; // Sửa thành namespace chứa ApplicationDbContext của bạn

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            // Add services to the container.

            

            // Sửa dòng builder.Services.AddControllers() thành:
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                // Bỏ qua lỗi vòng lặp khi convert dữ liệu sang JSON
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
