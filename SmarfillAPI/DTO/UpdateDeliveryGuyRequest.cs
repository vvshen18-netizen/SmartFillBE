namespace SmarfillAPI.DTO
{
    public class DeliveryGuyProfileUpdateDto
    {
        public string? NewUsername { get; set; }  // ✅ Optional
        public string? CurrentPassword { get; set; }  // ✅ Optional
        public string? NewPassword { get; set; }  // ✅ Optional
    }

}
