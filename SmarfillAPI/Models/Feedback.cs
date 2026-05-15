namespace SmarfillAPI.Models
{
    public class Feedback
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public int Rating { get; set; }
        public string Message { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string? AdminReply { get; set; }  // ✅ Make this nullable
    }
}
