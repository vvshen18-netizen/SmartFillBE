using AuthAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthAPI.Data; // Update with your actual namespace for DbContext
using SmarfillAPI.Models; // Update with your actual namespace for models
using System.Threading.Tasks;

namespace SmarfillAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryGuyController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DeliveryGuyController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /api/deliveryguy/check-status?email=example@email.com
        [HttpGet("check-status")]
        public async Task<IActionResult> CheckApprovalStatus([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email is required.");

            var deliveryGuy = await _context.DeliveryGuys
                .FirstOrDefaultAsync(d => d.Email == email);

            if (deliveryGuy == null)
                return NotFound("No delivery guy found with this email.");

            return Ok(new
            {
                Status = deliveryGuy.Status,
                RejectionReason = deliveryGuy.Status == "Rejected" ? deliveryGuy.RejectionReason : null
            });
        }

    }
}
