namespace AuthAPI.DTOs
{
    public class UpdateProfileRequest
    {
        public string? NewName { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
