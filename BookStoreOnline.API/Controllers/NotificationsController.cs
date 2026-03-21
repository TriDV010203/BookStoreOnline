using BookStoreOnline.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreOnline.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/notifications/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserNotification>>> GetUserNotifications(int userId)
        {
            return await _context.UserNotifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(30)
                .ToListAsync();
        }

        // GET: api/notifications/user/{userId}/unread-count
        [HttpGet("user/{userId}/unread-count")]
        public async Task<ActionResult> GetUnreadCount(int userId)
        {
            var count = await _context.UserNotifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
            return Ok(new { count });
        }

        // POST: api/notifications — Tạo thông báo mới
        [HttpPost]
        public async Task<ActionResult<UserNotification>> CreateNotification(UserNotification notification)
        {
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;
            notification.User = null;
            _context.UserNotifications.Add(notification);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUserNotifications), new { userId = notification.UserId }, notification);
        }

        // POST: api/notifications/broadcast — Gửi thông báo đến tất cả user hoặc nhiều user
        [HttpPost("broadcast")]
        public async Task<IActionResult> BroadcastNotification([FromBody] BroadcastRequest req)
        {
            var users = await _context.Users
                .Where(u => !u.IsBanned)
                .Select(u => u.Id)
                .ToListAsync();

            var notifications = users.Select(uid => new UserNotification
            {
                UserId = uid,
                Title = req.Title,
                Message = req.Message,
                Type = "Discount",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            _context.UserNotifications.AddRange(notifications);
            await _context.SaveChangesAsync();
            return Ok(new { sent = notifications.Count });
        }

        // PATCH: api/notifications/{id}/read
        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var notif = await _context.UserNotifications.FindAsync(id);
            if (notif == null) return NotFound();
            notif.IsRead = true;
            await _context.SaveChangesAsync();
            return Ok();
        }

        // PATCH: api/notifications/user/{userId}/read-all
        [HttpPatch("user/{userId}/read-all")]
        public async Task<IActionResult> MarkAllRead(int userId)
        {
            var notifs = await _context.UserNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
            notifs.ForEach(n => n.IsRead = true);
            await _context.SaveChangesAsync();
            return Ok(new { updated = notifs.Count });
        }
    }

    public class BroadcastRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Message { get; set; }
    }
}
