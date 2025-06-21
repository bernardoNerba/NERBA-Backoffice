using NERBABO.ApiService.Shared.Exceptions;
using NERBABO.ApiService.Shared.Models;
using System.Text.Json;

namespace NERBABO.ApiService.Shared.Middleware
{
    /// <summary>
    /// Middleware that handles unhandled exceptions globally and returns a standardized JSON error response.
    /// </summary>
    public class GlobalExceptionHandlerMiddleware : IMiddleware
    {
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly JsonSerializerOptions _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalExceptionHandlerMiddleware"/> class.
        /// </summary>
        /// <param name="logger">
        /// Logger used to record exception information.
        /// </param>
        public GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _logger = logger;
            _serializer = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        }

        /// <summary>
        /// Middleware execution method that wraps the request pipeline in a try-catch block.
        /// </summary>
        /// <param name="context">
        /// The current HTTP context for the request.
        /// </param>
        /// <param name="next">
        /// The next middleware delegate in the pipeline.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(context, exception);
            }
        }

        /// <summary>
        /// Handles known and unknown exceptions and returns a JSON-formatted error response.
        /// </summary>
        /// <param name="context">
        /// The HTTP context used to construct the response.
        /// </param>
        /// <param name="exception">
        /// The exception that was caught during request processing.
        /// </param>
        /// <returns>
        /// A task that completes when the error response has been written to the HTTP response.
        /// </returns>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var problemDetails = new ErrorMessage();

            switch (exception)
            {
                case KeyNotFoundException or ObjectNullException:
                    _logger.LogWarning(exception, "Resource not found: {Message}", exception.Message);
                    response.StatusCode = StatusCodes.Status404NotFound;
                    problemDetails.Message = "Recurso não encontrado.";
                    problemDetails.Details = exception.Message;
                    break;

                case InvalidOperationException:
                    _logger.LogWarning(exception, "Invalid operation: {Message}", exception.Message);
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    problemDetails.Message = "Operação inválida.";
                    problemDetails.Details = exception.Message;
                    break;

                case ArgumentNullException or ArgumentException:
                    _logger.LogWarning(exception, "Invalid argument: {Message}", exception.Message);
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    problemDetails.Message = "Parâmetros inválidos.";
                    problemDetails.Details = exception.Message;
                    break;

                case UnauthorizedAccessException:
                    _logger.LogWarning(exception, "Unauthorized access: {Message}", exception.Message);
                    response.StatusCode = StatusCodes.Status401Unauthorized;
                    problemDetails.Message = "Acesso não autorizado.";
                    break;

                case ValidationException validationEx:
                    _logger.LogWarning(exception, "Validation error: {Message}", exception.Message);
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    problemDetails.Message = "Erro de validação.";
                    problemDetails.Details = validationEx.Message;
                    break;

                default:
                    _logger.LogError(exception, "Unexpected error occurred: {Message}", exception.Message);
                    response.StatusCode = StatusCodes.Status500InternalServerError;
                    problemDetails.Message = "Erro interno do servidor.";
                    problemDetails.Details = "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.";

                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                    {
                        problemDetails.Details = exception.Message;
                        problemDetails.StackTrace = exception.StackTrace;
                    }
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(problemDetails, _serializer);
            await response.WriteAsync(jsonResponse);
        }
    }
}
