using System;
using System.ComponentModel.DataAnnotations;

namespace SmarfillAPI.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserEmail { get; set; }  // recipient of the notification

        [Required]
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;  // ✅ New field to track read/unread state

        public DateTime SentAt { get; set; } = DateTime.UtcNow;  // ✅ Default to current UTC
    }
}
