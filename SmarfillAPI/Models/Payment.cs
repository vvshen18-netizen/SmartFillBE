using System;

namespace SmarfillAPI.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public string UserEmail { get; set; }
        public string PaymentMethod { get; set; }

        // User final charge (Grand Total)
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }
        public TimeSpan PaymentTime { get; set; }

        // -------------------------------
        // SUBSIDY DETAILS
        // -------------------------------

        public bool IsSubsidyUsed { get; set; }

        public decimal NormalPricePerLitre { get; set; }
        public decimal SubsidyPricePerLitre { get; set; }

        public decimal SubsidizedLiters { get; set; }
        public decimal GovernmentSubsidy { get; set; }

        public decimal SubtotalBeforeSubsidy { get; set; }

        public decimal GrandTotal { get; set; }  // same as Amount
    }
}
