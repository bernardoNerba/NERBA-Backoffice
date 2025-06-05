namespace NERBABO.ApiService.Shared.Models
{
    public class ErrorMessage
    {
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? StackTrace { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
