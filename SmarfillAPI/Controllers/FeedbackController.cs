using AuthAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using SmarfillAPI.Models;

namespace SmarfillAPI.Controllers
{
    [ApiController]
    [Route("api/feedback")]
    public class FeedbackController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FeedbackController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitFeedback([FromBody] Feedback feedback)
        {
            if (string.IsNullOrWhiteSpace(feedback.UserEmail) || feedback.Rating <= 0 || string.IsNullOrWhiteSpace(feedback.Message))
            {
                return BadRequest("Invalid feedback data.");
            }

            feedback.SubmittedAt = DateTime.UtcNow;
            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return Ok("Feedback submitted successfully.");
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Feedback>>> GetAllFeedbacks()
        {
            var feedbacks = await _context.Feedbacks.OrderByDescending(f => f.SubmittedAt).ToListAsync();
            return Ok(feedbacks);
        }

        [HttpPut("reply/{id}")]
        public async Task<IActionResult> ReplyToFeedback(int id, [FromBody] string reply)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
                return NotFound();

            feedback.AdminReply = reply;
            await _context.SaveChangesAsync();

            return Ok("Reply saved successfully.");
        }

        [HttpGet("user/{email}")]
        public async Task<IActionResult> GetFeedbacksByUser(string email)
        {
            var feedbacks = await _context.Feedbacks
                .Where(f => f.UserEmail == email && f.AdminReply != null)
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();

            return Ok(feedbacks);
        }

        [HttpGet("total")]
        public async Task<IActionResult> GetTotalFeedbacks()
        {
            var count = await _context.Feedbacks.CountAsync();
            return Ok(count);
        }
    }

}
