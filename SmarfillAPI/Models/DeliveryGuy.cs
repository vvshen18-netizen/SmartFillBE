using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthAPI.Models
{
    public class DeliveryGuy
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string ContactNumber { get; set; }

        [Required]
        public string PasswordHash { get; set; } // ✅ Added for login verification

        [Required]
        public string LicensePhoto { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; // or "Approved"

        public string? RejectionReason { get; set; } // nullable

    }
}
