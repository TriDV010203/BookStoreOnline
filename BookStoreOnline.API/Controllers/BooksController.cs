using BookStoreOnline.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreOnline.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Lấy danh sách Sách (Có bao gồm luôn thông tin Danh mục)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            // Dùng .Include() để lấy kèm thông tin Category thay vì chỉ lấy CategoryId
            return await _context.Books.Include(b => b.Category).ToListAsync();
        }

        // 2. Lấy 1 cuốn sách theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            var book = await _context.Books
                                     .Include(b => b.Category) // Kéo theo thông tin danh mục
                                     .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return book;
        }

        // 3. Thêm sách mới
        [HttpPost]
        public async Task<ActionResult<Book>> PostBook(Book book)
        {
            // Xóa thông tin Category navigation để EF Core không cố gắng tạo Category mới
            book.Category = null;

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBook", new { id = book.Id }, book);
        }

        // 4. Sửa thông tin sách
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, Book book)
        {
            if (id != book.Id)
            {
                return BadRequest();
            }

            book.Category = null; // Tránh lỗi tracking của EF Core
            _context.Entry(book).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Books.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // 5. Xóa sách
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}