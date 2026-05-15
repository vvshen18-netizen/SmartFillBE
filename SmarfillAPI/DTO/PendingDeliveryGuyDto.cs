namespace AuthAPI.DTOs
{
    public class PendingDeliveryGuyDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public string? LicensePhoto { get; set; }
    }
}
