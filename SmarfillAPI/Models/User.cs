namespace AuthAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string ContactNumber { get; set; }
        public string Role { get; set; }
        public string? LicensePhoto { get; set; } // ✅ Make LicensePhoto nullable
        public string? ICFrontUrl { get; set; }
        public string? ICBackUrl { get; set; }
        public string ICVerificationStatus { get; set; } = "Pending";
        public DateTime? ICSubmittedAt { get; set; }

    }


}
