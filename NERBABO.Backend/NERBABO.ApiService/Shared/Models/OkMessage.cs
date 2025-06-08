namespace NERBABO.ApiService.Shared.Models;

public class OkMessage<T>
{
    public int? StatusCode { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public OkMessage() { }

    public OkMessage(string title, string message, int? statusCode = StatusCodes.Status200OK)
    {
        Title = title;
        Message = message;
        StatusCode = statusCode;
    }
    public OkMessage(string title, string message, T? data, int? statusCode = StatusCodes.Status200OK)
    {
        Title = title;
        Message = message;
        Data = data;
        StatusCode = statusCode;
    }
}
