using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmarfillAPI.Models; // Adjust namespace as needed
using AuthAPI.Data;   // Your DbContext namespace
using System;
using System.Linq;
using System.Threading.Tasks;
using AuthAPI.Data;

namespace SmarfillAPI.Controllers
{
    [ApiController]
    [Route("api/messages")]
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MessagesController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/messages/send
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.SenderEmail) || string.IsNullOrEmpty(dto.ReceiverEmail) || string.IsNullOrEmpty(dto.Message))
                return BadRequest("Invalid message data.");

            var message = new ChatMessage
            {
                SenderEmail = dto.SenderEmail,
                ReceiverEmail = dto.ReceiverEmail,
                Message = dto.Message,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // GET: api/messages/for-delivery-guy?email=example@domain.com
        [HttpGet("for-delivery-guy")]
        public async Task<IActionResult> GetMessagesForDeliveryGuy([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email is required.");

            var messages = await _context.ChatMessages
                .Where(m => m.ReceiverEmail == email)
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();

            return Ok(messages);
        }

        // POST: api/messages/mark-read
        [HttpPost("mark-read")]
        public async Task<IActionResult> MarkMessagesAsRead([FromBody] int[] messageIds)
        {
            if (messageIds == null || messageIds.Length == 0)
                return BadRequest("No message IDs provided.");

            var messages = await _context.ChatMessages
                .Where(m => messageIds.Contains(m.Id))
                .ToListAsync();

            if (messages.Count == 0)
                return NotFound("No matching messages found.");

            foreach (var message in messages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return Ok("Messages marked as read.");
        }

    }

    // DTO class to receive message data
    public class ChatMessageDto
    {
        public string SenderEmail { get; set; }
        public string ReceiverEmail { get; set; }
        public string Message { get; set; }
    }
}
