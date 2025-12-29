using NERBABO.ApiService.Core.Authentication.Services;

namespace NERBABO.ApiService.Shared.Middleware;

/// <summary>
/// Middleware to check if JWT tokens are blacklisted (invalidated)
/// This middleware runs after authentication to verify tokens haven't been logged out
/// </summary>
public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenBlacklistMiddleware> _logger;

    public TokenBlacklistMiddleware(
        RequestDelegate next,
        ILogger<TokenBlacklistMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context, ITokenBlacklistService tokenBlacklistService)
    {
        // Only check authenticated requests
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Get the token from the Authorization header
            var authHeader = context.Request.Headers.Authorization.ToString();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader["Bearer ".Length..].Trim();

                // Check if token is blacklisted
                var isBlacklisted = await tokenBlacklistService.IsTokenBlacklistedAsync(token);

                if (isBlacklisted)
                {
                    _logger.LogWarning("Blacklisted token detected, rejecting request");

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = "Token has been invalidated. Please login again.",
                        statusCode = 401
                    });

                    return; // Stop processing the request
                }
            }
        }

        // Token is valid or request is not authenticated, continue pipeline
        await _next(context);
    }
}
