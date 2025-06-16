using NERBABO.ApiService.Shared.Exceptions;
using NERBABO.ApiService.Shared.Models;
using System.Text.Json;

namespace NERBABO.ApiService.Shared.Middleware
{
    public class GlobalExceptionHandlerMiddleware : IMiddleware
    {
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly JsonSerializerOptions _serializer;

        public GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _logger = logger;
            _serializer = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        }

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

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = new ErrorMessage();

            switch (exception)
            {
                case KeyNotFoundException or ObjectNullException:
                    _logger.LogWarning(exception, "Resource not found: {Message}", exception.Message);
                    response.StatusCode = StatusCodes.Status404NotFound;
                    errorResponse.Message = "Recurso não encontrado.";
                    errorResponse.Details = exception.Message;
                    break;

                case InvalidOperationException:
                    _logger.LogWarning(exception, "Invalid operation: {Message}", exception.Message);
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    errorResponse.Message = "Operação inválida.";
                    errorResponse.Details = exception.Message;
                    break;

                case ArgumentNullException or ArgumentException:
                    _logger.LogWarning(exception, "Invalid argument: {Message}", exception.Message);
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    errorResponse.Message = "Parâmetros inválidos.";
                    errorResponse.Details = exception.Message;
                    break;

                case UnauthorizedAccessException:
                    _logger.LogWarning(exception, "Unauthorized access: {Message}", exception.Message);
                    response.StatusCode = StatusCodes.Status401Unauthorized;
                    errorResponse.Message = "Acesso não autorizado.";
                    break;

                case ValidationException validationEx:
                    _logger.LogWarning(exception, "Validation error: {Message}", exception.Message);
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    errorResponse.Message = "Erro de validação.";
                    errorResponse.Details = validationEx.Message;
                    break;

                default:
                    _logger.LogError(exception, "Unexpected error occurred: {Message}", exception.Message);
                    response.StatusCode = StatusCodes.Status500InternalServerError;
                    errorResponse.Message = "Erro interno do servidor.";
                    errorResponse.Details = "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.";

                    // Don't expose internal error details in production
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                    {
                        errorResponse.Details = exception.Message;
                        errorResponse.StackTrace = exception.StackTrace;
                    }
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(errorResponse, _serializer);

            await response.WriteAsync(jsonResponse);
        }
    }
}