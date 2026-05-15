using Microsoft.AspNetCore.Mvc;
using SmarfillAPI.Models;
using AuthAPI.Data;
using Microsoft.EntityFrameworkCore;
using SmarfillAPI.DTO;

namespace SmarfillAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentController(AppDbContext context)
        {
            _context = context;
        }

        // ===============================================================
        // SUBMIT PAYMENT (Support Subsidy & Normal Pricing)
        // ===============================================================
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitPayment([FromBody] PaymentDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid payment data.");

            // Map DTO → Payment Model
            var payment = new Payment
            {
                UserEmail = dto.UserEmail,
                PaymentMethod = dto.PaymentMethod,
                Amount = dto.Amount,
                PaymentDate = dto.PaymentDate,
                PaymentTime = dto.PaymentTime,

                // -------------------------
                // SUBSIDY FIELDS
                // -------------------------
                IsSubsidyUsed = dto.IsSubsidyUsed,
                NormalPricePerLitre = dto.NormalPricePerLitre,
                SubsidyPricePerLitre = dto.SubsidyPricePerLitre,
                SubsidizedLiters = dto.SubsidizedLiters,
                GovernmentSubsidy = dto.GovernmentSubsidy,
                SubtotalBeforeSubsidy = dto.SubtotalBeforeSubsidy,
                GrandTotal = dto.GrandTotal
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Payment saved successfully.",
                paymentId = payment.PaymentId
            });
        }

        // ===============================================================
        // GET ALL PAYMENTS
        // ===============================================================
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetAllPayments()
        {
            return await _context.Payments
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.PaymentTime)
                .ToListAsync();
        }
    }
}
