using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmarfillAPI.Models;
using AuthAPI.Data;

namespace SmarfillAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Save a new notification
        [HttpPost("save")]
        public async Task<IActionResult> SaveNotification([FromBody] Notification notification)
        {
            if (string.IsNullOrWhiteSpace(notification.UserEmail) || string.IsNullOrWhiteSpace(notification.Message))
                return BadRequest("Email and message are required.");

            notification.SentAt = DateTime.UtcNow;

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok("Notification saved successfully.");
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] Notification notification)
        {
            notification.SentAt = DateTime.UtcNow;
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return Ok();
        }


        // ✅ Get notifications for a specific user
        [HttpGet("user/{email}")]
        public async Task<IActionResult> GetUserNotifications(string email)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserEmail == email)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();

            return Ok(notifications);
        }

        // (Optional) Delete all notifications for a user
        [HttpDelete("user/{email}")]
        public async Task<IActionResult> DeleteNotifications(string email)
        {
            var userNotifs = await _context.Notifications
                .Where(n => n.UserEmail == email)
                .ToListAsync();

            if (!userNotifs.Any()) return NotFound("No notifications found.");

            _context.Notifications.RemoveRange(userNotifs);
            await _context.SaveChangesAsync();

            return Ok("User notifications deleted.");
        }

        // (Optional) Mark a notification as read
        [HttpPut("mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return NotFound("Notification not found.");

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok("Marked as read.");
        }

        [HttpPost("accept/{orderId}")]
        public async Task<IActionResult> AcceptOrder(int orderId, [FromBody] string deliveryGuyEmail)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound();

            order.Status = "Accepted";
            order.AssignedDeliveryGuyEmail = deliveryGuyEmail;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("reject/{orderId}")]
        public async Task<IActionResult> RejectOrder(int orderId, [FromBody] RejectRequest request)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound();

            order.Status = "Rejected";
            order.AssignedDeliveryGuyEmail = request.DeliveryGuyEmail;

            await _context.SaveChangesAsync();
            return Ok();
        }

        public class RejectRequest
        {
            public string DeliveryGuyEmail { get; set; }
            public string Reason { get; set; }
        }

    }
}
