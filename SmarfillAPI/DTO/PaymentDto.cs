namespace SmarfillAPI.DTO
{
    public class PaymentDto
    {
        public string UserEmail { get; set; }
        public string PaymentMethod { get; set; }

        public decimal Amount { get; set; }

        // Subsidy Fields
        public bool IsSubsidyUsed { get; set; }

        public decimal NormalPricePerLitre { get; set; }
        public decimal SubsidyPricePerLitre { get; set; }
        public decimal SubsidizedLiters { get; set; }

        public decimal GovernmentSubsidy { get; set; }
        public decimal SubtotalBeforeSubsidy { get; set; }
        public decimal GrandTotal { get; set; }

        public DateTime PaymentDate { get; set; }
        public TimeSpan PaymentTime { get; set; }
    }
}
