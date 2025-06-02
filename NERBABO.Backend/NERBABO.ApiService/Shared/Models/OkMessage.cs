namespace NERBABO.ApiService.Shared.Models;

public class OkMessage
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; } = null;

    public OkMessage() { }
    public OkMessage(string title, string message, object? data)
    {
        Title = title;
        Message = message;
        Data = data;
    }
}
