using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmarfillAPI.Models;
using AuthAPI.Data;
using SmarfillAPI.DTO;

namespace SmarfillAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DeliveryController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Route used by your app (submit delivery)
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitDelivery([FromBody] Delivery delivery)
        {
            try
            {
                _context.Deliveries.Add(delivery);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Delivery submitted." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to submit delivery.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // ✅ Alternative route (you can still use this if needed)
        [HttpPost("mark-delivered")]
        public async Task<IActionResult> MarkAsDelivered([FromBody] Delivery delivery)
        {
            try
            {
                // ✅ Accept any delivery stage/status
                _context.Deliveries.Add(delivery);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Delivery saved." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to save delivery record.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }


        // ✅ Get all deliveries made by a specific delivery guy
        [HttpGet("by-delivery-guy/{email}")]
        public async Task<IActionResult> GetByDeliveryGuy(string email)
        {
            var deliveries = await _context.Deliveries
                .Where(d => d.DeliveryGuyEmail == email && (d.Status == "Delivered" || d.Status == "Rejected"))
                .ToListAsync();

            return Ok(deliveries);
        }


        // ✅ Get OrderIds that have already been delivered (for filtering)
        [HttpGet("delivered-orders/{deliveryGuyEmail}")]
        public async Task<IActionResult> GetDeliveredOrderIds(string deliveryGuyEmail)
        {
            var deliveredIds = await _context.Deliveries
                .Where(d => d.DeliveryGuyEmail == deliveryGuyEmail && d.Status == "Delivered")
                .Select(d => d.OrderId)
                .ToListAsync();

            return Ok(deliveredIds);
        }

        [HttpGet("trips/{deliveryGuyEmail}")]
        public async Task<IActionResult> GetTripsByDeliveryGuy(string deliveryGuyEmail)
        {
            var trips = await _context.Deliveries
                .Where(d => d.DeliveryGuyEmail == deliveryGuyEmail)
                .ToListAsync();

            return Ok(trips);
        }

        [HttpGet("trip-count/{deliveryGuyEmail}")]
        public async Task<IActionResult> GetTripCount(string deliveryGuyEmail)
        {
            var count = await _context.Deliveries
                .Where(d => d.DeliveryGuyEmail == deliveryGuyEmail && d.Status == "Delivered")
                .CountAsync();

            return Ok(count);
        }

        [HttpGet("total-deliveries")]
        public async Task<IActionResult> GetTotalDeliveredOrders()
        {
            var count = await _context.Deliveries.CountAsync();
            return Ok(count);
        }

        [HttpGet("total-earnings")]
        public async Task<IActionResult> GetTotalEarningsFromDeliveries()
        {
            var total = await _context.Deliveries.SumAsync(d => d.Amount);
            return Ok(total);
        }

        [HttpGet("delivered-orders-history")]
        public async Task<IActionResult> GetDeliveredOrdersHistory()
        {
            var deliveries = await _context.Deliveries
                .OrderByDescending(d => d.DeliveryDate)
                .ToListAsync();

            return Ok(deliveries);
        }

        [HttpPut("update-tracking/{deliveryId}")]
        public async Task<IActionResult> UpdateTrackingStage(int deliveryId, [FromBody] int stage)
        {
            var delivery = await _context.Deliveries.FindAsync(deliveryId);
            if (delivery == null) return NotFound("Delivery not found");

            delivery.TrackingStage = stage;
            await _context.SaveChangesAsync();

            return Ok("Tracking stage updated");
        }


        [HttpGet("by-order/{orderId}")]
        public async Task<IActionResult> GetDeliveryByOrderId(int orderId)
        {
            var delivery = await _context.Deliveries.FirstOrDefaultAsync(d => d.OrderId == orderId);
            if (delivery == null) return NotFound();

            return Ok(delivery);
        }

        [HttpGet("latest-for-user/{email}")]
        public async Task<IActionResult> GetLatestOrderForUser(string email)
        {
            var order = await _context.Orders
                .Where(o => o.UserEmail == email) // remove .Status filter
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderTime)
                .FirstOrDefaultAsync();

            return order != null ? Ok(order) : NotFound();
        }


        [HttpGet("is-delivered/{orderId}")]
        public async Task<IActionResult> IsDelivered(int orderId)
        {
            var delivered = await _context.Deliveries
                .AnyAsync(d => d.OrderId == orderId && d.Status == "Delivered");
            return Ok(delivered);
        }

        [HttpGet("on-the-way/{orderId}")]
        public async Task<IActionResult> IsOnTheWay(int orderId)
        {
            var exists = await _context.Deliveries
                .AnyAsync(d => d.OrderId == orderId && d.TrackingStage == 2); // ✅ Must match stage 2
            return Ok(exists);
        }


        [HttpPut("update-tracking-by-order/{orderId}")]
        public async Task<IActionResult> UpdateTrackingStageByOrderId(int orderId, [FromBody] UpdateTrackingDto dto)
        {
            var delivery = await _context.Deliveries.FirstOrDefaultAsync(d => d.OrderId == orderId);
            if (delivery == null) return NotFound("Delivery not found");

            delivery.TrackingStage = dto.Stage;
            await _context.SaveChangesAsync();

            return Ok("Tracking updated");
        }

        [HttpPut("mark-delivered/{orderId}")]
        public async Task<IActionResult> MarkDeliveryAsDelivered(int orderId)
        {
            var delivery = await _context.Deliveries.FirstOrDefaultAsync(d => d.OrderId == orderId);
            if (delivery == null) return NotFound("Delivery not found.");

            delivery.TrackingStage = 3;
            delivery.Status = "Delivered";

            await _context.SaveChangesAsync();
            return Ok("Delivery marked as delivered.");
        }

        [HttpGet("report-by-delivery/{email}")]
        public async Task<IActionResult> GetReportByDeliveryGuy(string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email is required.");

            var orders = await _context.Deliveries
                .Where(d => d.DeliveryGuyEmail == email && d.Status == "Delivered")
                .Select(d => new
                {
                    d.CustomerEmail,
                    d.OrderType,
                    d.DeliveryAddress,
                    d.FuelType,
                    d.Amount,
                    d.PaymentMethod,
                    d.DeliveryDate,
                    d.Status
                }).ToListAsync();

            if (orders == null || orders.Count == 0)
                return NotFound("No delivered orders found for this delivery guy.");

            return Ok(orders);
        }

    }
}
