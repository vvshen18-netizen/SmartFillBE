using Microsoft.AspNetCore.Mvc;
using SmarfillAPI.Models;
using AuthAPI.Data;
using Microsoft.EntityFrameworkCore;
using SmarfillAPI.DTO;

namespace SmarfillAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        // ====================================================================
        // ✅ SUBMIT ORDER (SUPPORTS SUBSIDY)
        // ====================================================================
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitOrder([FromBody] Order order)
        {
            try
            {
                // ---------------------------------------------------------------
                // 1️⃣ Auto-approve payment if CASH
                // ---------------------------------------------------------------
                if (order.PaymentMethod == "Cash")
                    order.IsPaymentApproved = true;

                // ---------------------------------------------------------------
                // 2️⃣ Validate subsidy fields
                // ---------------------------------------------------------------
                if (!order.IsSubsidyUsed)
                {
                    // Force subsidy values to zero for normal price orders
                    order.SubsidyPricePerLitre = 0;
                    order.SubsidizedLiters = 0;
                    order.GovernmentSubsidy = 0;

                    order.SubtotalBeforeSubsidy = order.Amount;
                    order.GrandTotal = order.Amount;
                }
                else
                {
                    // Round subsidy invoice values
                    order.SubsidizedLiters = Math.Round(order.SubsidizedLiters, 3);
                    order.GovernmentSubsidy = Math.Round(order.GovernmentSubsidy, 2);
                    order.SubtotalBeforeSubsidy = Math.Round(order.SubtotalBeforeSubsidy, 2);
                    order.GrandTotal = Math.Round(order.GrandTotal, 2);
                }

                // ---------------------------------------------------------------
                // 3️⃣ Save the ORDER
                // ---------------------------------------------------------------
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // ---------------------------------------------------------------
                // 4️⃣ Create initial DELIVERY record
                // ---------------------------------------------------------------
                var delivery = new Delivery
                {
                    OrderId = order.OrderId,
                    CustomerEmail = order.UserEmail,
                    DeliveryGuyEmail = "",
                    OrderType = order.OrderType,
                    DeliveryAddress = order.DeliveryAddress,
                    DeliveryDate = order.OrderDate,
                    DeliveryTime = order.OrderTime,
                    FuelType = order.FuelType,
                    Amount = order.Amount,
                    PaymentMethod = order.PaymentMethod,
                    Status = "Placed",
                    TrackingStage = 0
                };

                _context.Deliveries.Add(delivery);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Order submitted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to submit order.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // ====================================================================
        // GET ALL ORDERS
        // ====================================================================
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
        {
            return await _context.Orders.ToListAsync();
        }

        // ====================================================================
        // TOTAL COMPLETED ORDERS
        // ====================================================================
        [HttpGet("total")]
        public async Task<IActionResult> GetTotalOrders()
        {
            var totalOrders = await _context.Orders
                .Where(o => o.Status == "DeliveredAndConfirmed")
                .CountAsync();

            return Ok(totalOrders);
        }

        // ====================================================================
        // TOTAL EARNINGS
        // ====================================================================
        [HttpGet("total-earnings")]
        public async Task<IActionResult> GetTotalEarnings()
        {
            var totalEarnings = await _context.Orders
                .Where(o => o.Status == "DeliveredAndConfirmed")
                .SumAsync(o => o.Amount);

            return Ok(totalEarnings);
        }

        // ====================================================================
        // ACCEPT ORDER (DELIVERY GUY)
        // ====================================================================
        [HttpPost("accept")]
        public async Task<IActionResult> AcceptOrder([FromBody] AcceptOrderDto dto)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == dto.OrderId);
            if (order == null) return NotFound("Order not found");

            order.Status = "Accepted";
            order.AssignedDeliveryGuyEmail = dto.DeliveryGuyEmail;

            var existingDelivery = await _context.Deliveries.FirstOrDefaultAsync(d => d.OrderId == dto.OrderId);

            if (existingDelivery != null)
            {
                existingDelivery.DeliveryGuyEmail = dto.DeliveryGuyEmail;
                existingDelivery.Status = "Accepted";
                existingDelivery.TrackingStage = 1;
            }
            else
            {
                var delivery = new Delivery
                {
                    OrderId = dto.OrderId,
                    DeliveryGuyEmail = dto.DeliveryGuyEmail,
                    Status = "Accepted",
                    TrackingStage = 1,
                    DeliveryDate = DateTime.Now,
                    FuelType = order.FuelType,
                    Amount = order.Amount,
                    PaymentMethod = order.PaymentMethod,
                    DeliveryAddress = order.DeliveryAddress,
                    CustomerEmail = order.UserEmail,
                    OrderType = order.OrderType
                };

                _context.Deliveries.Add(delivery);
            }

            await _context.SaveChangesAsync();
            return Ok("Order accepted");
        }

        // ====================================================================
        // REJECT ORDER FOR SPECIFIC DELIVERY GUY
        // ====================================================================
        [HttpPut("reject/{orderId}")]
        public async Task<IActionResult> RejectOrder(int orderId, [FromBody] RejectOrderDto dto)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound("Order not found.");

            var alreadyRejected = await _context.OrderRejections
                .AnyAsync(r => r.OrderId == orderId && r.DeliveryGuyEmail == dto.DeliveryGuyEmail);

            if (!alreadyRejected)
            {
                _context.OrderRejections.Add(new OrderRejection
                {
                    OrderId = orderId,
                    DeliveryGuyEmail = dto.DeliveryGuyEmail
                });
                await _context.SaveChangesAsync();
            }

            return Ok("Order rejected for this delivery guy.");
        }

        // ====================================================================
        // UPDATE ORDER STATUS
        // ====================================================================
        [HttpPut("update-status/{orderId}")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] string newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.Status = newStatus;
            await _context.SaveChangesAsync();
            return Ok();
        }

        // ====================================================================
        // AVAILABLE ORDERS FOR DELIVERY GUY
        // ====================================================================
        [HttpGet("available-for/{email}")]
        public async Task<IActionResult> GetAvailableOrdersForDeliveryGuy(string email)
        {
            var rejectedIds = await _context.OrderRejections
                .Where(r => r.DeliveryGuyEmail == email)
                .Select(r => r.OrderId)
                .ToListAsync();

            var orders = await _context.Orders
                .Where(o =>
                    o.IsPaymentApproved &&
                    string.IsNullOrEmpty(o.AssignedDeliveryGuyEmail) &&
                    o.Status == "Pending" &&
                    !rejectedIds.Contains(o.OrderId))
                .ToListAsync();

            return Ok(orders);
        }

        // ====================================================================
        // MARK ORDER AS DELIVERED
        // ====================================================================
        [HttpPut("mark-delivered/{orderId}")]
        public async Task<IActionResult> MarkAsDelivered(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound("Order not found.");

            if (order.Status != "Accepted")
                return BadRequest("Only accepted orders can be delivered.");

            order.Status = "Delivered";
            await _context.SaveChangesAsync();
            return Ok("Order delivered.");
        }

        // ====================================================================
        // GET LATEST ORDER FOR USER
        // ====================================================================
        [HttpGet("latest-for-user/{email}")]
        public async Task<IActionResult> GetLatestOrderForUser(string email)
        {
            var order = await _context.Orders
                .Where(o => o.UserEmail == email && o.Status != "DeliveredAndConfirmed")
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderTime)
                .Select(o => new
                {
                    o.OrderId,
                    o.UserEmail,
                    o.OrderType,
                    o.OrderDate,
                    o.OrderTime,
                    o.DeliveryAddress,
                    o.FuelType,
                    o.Amount,
                    o.PaymentMethod,
                    o.AssignedDeliveryGuyEmail,
                    o.Status,
                    DeliveryGuyName = _context.DeliveryGuys
                        .Where(d => d.Email == o.AssignedDeliveryGuyEmail)
                        .Select(d => d.Username)
                        .FirstOrDefault(),
                    DeliveryGuyContactNumber = _context.DeliveryGuys
                        .Where(d => d.Email == o.AssignedDeliveryGuyEmail)
                        .Select(d => d.ContactNumber)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound();

            return Ok(order);
        }
    }
}
