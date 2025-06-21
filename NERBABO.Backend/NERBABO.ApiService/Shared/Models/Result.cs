namespace NERBABO.ApiService.Shared.Models;

/// <summary>
/// Non-generic base class for representing success or failure of an operation.
/// </summary>
public class Result
{
    public bool Success { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public int? StatusCode { get; set; }
    public object? Errors { get; set; }

    /// <summary>
    /// Creates a successful result with message and status code.
    /// </summary>
    /// <param name="title">
    /// A short title describing the result.
    /// </param>
    /// <param name="message">
    /// A more detailed message describing the result.
    /// </param>
    /// <param name="status">
    /// HTTP status code (must be in the 2xx range).
    /// </param>
    /// <returns>
    /// Returns a successful <see cref="Result"/>.
    /// </returns>
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

    /// <summary>
    /// Creates a failed result with a title and message.
    /// </summary>
    /// <param name="title">
    /// A short title describing the error.
    /// </param>
    /// <param name="message">
    /// A detailed error message.
    /// </param>
    /// <param name="statusCode">
    /// HTTP status code for the error (defaults to 400).
    /// </param>
    /// <returns>
    /// Returns a failed <see cref="Result"/>.
    /// </returns>
    public static Result Fail(string title, string message, int statusCode = StatusCodes.Status400BadRequest) =>
        new()
        {
            Success = false,
            Title = title,
            Message = message,
            StatusCode = statusCode
        };

    /// <summary>
    /// Creates a failed result with additional error data.
    /// </summary>
    /// <param name="title">
    /// A short title describing the error.
    /// </param>
    /// <param name="message">
    /// A detailed error message.
    /// </param>
    /// <param name="errors">
    /// An object containing extra error details.
    /// </param>
    /// <param name="statusCode">
    /// HTTP status code for the error (defaults to 400).
    /// </param>
    /// <returns>
    /// Returns a failed <see cref="Result"/> with error metadata.
    /// </returns>
    public static Result Fail(string title, string message, object errors, int statusCode = StatusCodes.Status400BadRequest) =>
        new()
        {
            Success = false,
            Title = title,
            Message = message,
            StatusCode = statusCode,
            Errors = errors
        };
}


/// <summary>
/// Generic result class that encapsulates operation outcomes and data for successful responses.
/// </summary>
/// <typeparam name="T">The type of the response data.</typeparam>
public class Result<T> : Result
{
    public T? Data { get; set; }

    /// <summary>
    /// Creates a successful result with data, title, and message.
    /// </summary>
    /// <param name="data">
    /// The data payload returned by the operation.
    /// </param>
    /// <param name="title">
    /// A short title describing the result.
    /// </param>
    /// <param name="message">
    /// A more detailed message describing the result.
    /// </param>
    /// <returns>
    /// Returns a successful <see cref="Result{T}"/> with HTTP 200 status.
    /// </returns>
    public static Result<T> Ok(T data, string title, string message) => new()
    {
        Success = true,
        Title = title,
        Message = message,
        StatusCode = StatusCodes.Status200OK,
        Data = data
    };

    /// <summary>
    /// Creates a successful result with data, title, message, and custom status code.
    /// </summary>
    /// <param name="data">
    /// The data payload returned by the operation.
    /// </param>
    /// <param name="title">
    /// A short title describing the result.
    /// </param>
    /// <param name="message">
    /// A more detailed message describing the result.
    /// </param>
    /// <param name="status">
    /// HTTP status code (must be in the 2xx range).
    /// </param>
    /// <returns>
    /// Returns a successful <see cref="Result{T}"/> with custom status.
    /// </returns>
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

    /// <summary>
    /// Creates a basic successful result with data only.
    /// </summary>
    /// <param name="data">
    /// The data payload returned by the operation.
    /// </param>
    /// <returns>
    /// Returns a successful <see cref="Result{T}"/> with default status 200.
    /// </returns>
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

    /// <summary>
    /// Creates a failed result with title and message.
    /// </summary>
    /// <param name="title">
    /// A short title describing the error.
    /// </param>
    /// <param name="message">
    /// A detailed error message.
    /// </param>
    /// <param name="statusCode">
    /// HTTP status code for the error (defaults to 400).
    /// </param>
    /// <returns>
    /// Returns a failed <see cref="Result{T}"/>.
    /// </returns>
    public new static Result<T> Fail(string title, string message, int statusCode = StatusCodes.Status400BadRequest) =>
    new()
    {
        Success = false,
        Title = title,
        Message = message,
        StatusCode = statusCode,
        Data = default
    };

    /// <summary>
    /// Creates a failed result with additional validation or error data.
    /// </summary>
    /// <param name="title">
    /// A short title describing the error.
    /// </param>
    /// <param name="message">
    /// A detailed error message.
    /// </param>
    /// <param name="errors">
    /// Additional error details (e.g., model validation results).
    /// </param>
    /// <param name="statusCode">
    /// HTTP status code for the error (defaults to 400).
    /// </param>
    /// <returns>
    /// Returns a failed <see cref="Result{T}"/> with error metadata.
    /// </returns>
    public new static Result<T> Fail(string title, string message, object errors, int statusCode = StatusCodes.Status400BadRequest) =>
    new()
    {
        Success = false,
        Title = title,
        Message = message,
        StatusCode = statusCode,
        Errors = errors,
        Data = default
    };
}