using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmarfillAPI.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public string UserEmail { get; set; }
        public string OrderType { get; set; }
        public DateTime OrderDate { get; set; }
        public TimeSpan OrderTime { get; set; }
        public string DeliveryAddress { get; set; }
        public string FuelType { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string? AssignedDeliveryGuyEmail { get; set; }
        public string Status { get; set; } = "Pending";
        public bool IsPaymentApproved { get; set; } = false;
        public string? ReceiptImage { get; set; }

        // ===============================
        // NEW SUBSIDY FIELDS
        // ===============================
        public bool IsSubsidyUsed { get; set; } = false;

        public decimal NormalPricePerLitre { get; set; }
        public decimal SubsidyPricePerLitre { get; set; }

        [Column(TypeName = "decimal(10,3)")]
        public decimal SubsidizedLiters { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal GovernmentSubsidy { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal SubtotalBeforeSubsidy { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal GrandTotal { get; set; }
    }

}
