using BookStoreOnline.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreOnline.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Lấy danh sách toàn bộ người dùng (Dành cho Admin)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // 2. Đăng ký tài khoản mới
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(User user)
        {
            // Kiểm tra email đã tồn tại chưa
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return BadRequest("Email này đã được sử dụng!");
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUsers", new { id = user.Id }, user);
        }

        // 3. Đăng nhập (Tạm thời check trực tiếp, Phase sau sẽ nâng cấp lên JWT Token)
        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.PasswordHash == request.Password);

            if (user == null)
            {
                return Unauthorized("Email hoặc mật khẩu không chính xác!");
            }

            return Ok(user);
        }
    }

    // Class phụ để hứng dữ liệu lúc đăng nhập
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}