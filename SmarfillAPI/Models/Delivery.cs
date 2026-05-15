using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmarfillAPI.Models
{
    public class Delivery
    {
        [Key]
        public int DeliveryId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public string CustomerEmail { get; set; }

        [Required]
        public string DeliveryGuyEmail { get; set; }

        [Required]
        public string OrderType { get; set; }

        [Required]
        public string DeliveryAddress { get; set; }

        [Required]
        public DateTime DeliveryDate { get; set; }

        [Required]
        public TimeSpan DeliveryTime { get; set; }

        [Required]
        public string FuelType { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; }

        public string Status { get; set; } = "Accepted";

        public int TrackingStage { get; set; } = 1; // 1: Placed, 2: Accepted, 3: On the way, 4: Delivered
    }
}
