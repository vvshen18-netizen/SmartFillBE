namespace AuthAPI.DTOs
{
    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ContactNumber { get; set; }
        public string Role { get; set; } // ✅ Important

        public string? LicensePhoto { get; set; } // ✅ add this (nullable)
    }
}