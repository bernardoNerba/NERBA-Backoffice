using System.Reflection;

namespace NERBABO.ApiService.Shared.Models
{
    public class Result<T>: Result
    {
        public T? Data { get; set; }

        public static Result<T> Ok (T data, string title, string message) =>
            new() 
            { 
                Success = true, 
                Title = title, 
                Message = message, 
                StatusCode = StatusCodes.Status200OK, 
                Data = data 
            };

        public static Result<T> Ok(T data, string title, string message, int status)
        {
            if (status < 200 || status >= 300)
                throw new ArgumentOutOfRangeException(nameof(status), 
                    $"Only 2xx status codes are allowed for a successful result. Received: {status}");
            
            return new() 
            { 
                Success = true, 
                Title = title, 
                Message = message, 
                StatusCode = status, 
                Data = data 
            };
        }

        public static Result<T> Ok(T data)
        {
            return new()
            {
                Success = true,
                Title = null,
                Message = null,
                StatusCode = StatusCodes.Status200OK,
                Data = data
            };
        }

        public new static Result<T> Fail(string title, string message, int statusCode = StatusCodes.Status400BadRequest) =>
        new()
        {
            Success = false,
            Title = title,
            Message = message,
            StatusCode = statusCode,
            Data = default
        };
    }
    public class Result
    {
        public bool Success { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public int? StatusCode { get; set; }

        public static Result Ok(string title, string message, int status = StatusCodes.Status200OK)
        {
            if (status < 200 || status >= 300)
                throw new ArgumentOutOfRangeException(nameof(status), $"Only 2xx status codes are allowed. Received: {status}");

            return new Result
            {
                Success = true,
                Title = title,
                Message = message,
                StatusCode = status
            };
        }

        public static Result Fail(string title, string message, int statusCode = StatusCodes.Status400BadRequest) =>
            new()
            {
                Success = false,
                Title = title,
                Message = message,
                StatusCode = statusCode
            };
    }


}
