using System.Text.RegularExpressions;

namespace GymHero.Api.Middleware;

/// <summary>
/// Middleware to validate input and detect potential SQL injection and XSS attacks
/// </summary>
public class InputValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<InputValidationMiddleware> _logger;

    // SQL injection patterns - common SQL keywords and syntax
    private static readonly Regex SqlInjectionPattern = new(
        @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE|UNION|DECLARE|CAST|CONVERT)\b)|(--|;|\/\*|\*\/|xp_|sp_|'(\s)*(OR|AND))",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    // XSS patterns - script tags and JavaScript injection attempts
    private static readonly Regex XssPattern = new(
        @"(<script|<iframe|javascript:|onerror=|onload=|onclick=|onmouseover=|<embed|<object|eval\(|alert\()",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    // Path traversal patterns
    private static readonly Regex PathTraversalPattern = new(
        @"(\.\./|\.\.\\|%2e%2e)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    public InputValidationMiddleware(RequestDelegate next, ILogger<InputValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip validation for static files and health checks
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.StartsWith("/health") ||
            path.StartsWith("/_framework") ||
            path.Contains(".css") ||
            path.Contains(".js") ||
            path.Contains(".map"))
        {
            await _next(context);
            return;
        }

        // Validate query parameters
        foreach (var param in context.Request.Query)
        {
            var value = param.Value.ToString();

            if (string.IsNullOrWhiteSpace(value))
                continue;

            // Skip validation for JWT tokens (used in SignalR authentication)
            if (param.Key.Equals("access_token", StringComparison.OrdinalIgnoreCase) ||
                param.Key.Equals("id_token", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Check for SQL injection
            if (SqlInjectionPattern.IsMatch(value))
            {
                _logger.LogWarning(
                    "Potential SQL injection detected. IP: {IP}, Path: {Path}, Parameter: {Key}, Value: {Value}",
                    context.Connection.RemoteIpAddress,
                    context.Request.Path,
                    param.Key,
                    value
                );

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Invalid input detected. Please check your request parameters.",
                    error = "VALIDATION_ERROR"
                });
                return;
            }

            // Check for XSS
            if (XssPattern.IsMatch(value))
            {
                _logger.LogWarning(
                    "Potential XSS attack detected. IP: {IP}, Path: {Path}, Parameter: {Key}, Value: {Value}",
                    context.Connection.RemoteIpAddress,
                    context.Request.Path,
                    param.Key,
                    value
                );

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Invalid input detected. Please check your request parameters.",
                    error = "VALIDATION_ERROR"
                });
                return;
            }

            // Check for path traversal
            if (PathTraversalPattern.IsMatch(value))
            {
                _logger.LogWarning(
                    "Potential path traversal attack detected. IP: {IP}, Path: {Path}, Parameter: {Key}, Value: {Value}",
                    context.Connection.RemoteIpAddress,
                    context.Request.Path,
                    param.Key,
                    value
                );

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Invalid input detected. Please check your request parameters.",
                    error = "VALIDATION_ERROR"
                });
                return;
            }
        }

        // Validate route parameters
        foreach (var routeValue in context.Request.RouteValues)
        {
            var value = routeValue.Value?.ToString();

            if (string.IsNullOrWhiteSpace(value))
                continue;

            // Check for SQL injection in route parameters
            if (SqlInjectionPattern.IsMatch(value))
            {
                _logger.LogWarning(
                    "Potential SQL injection in route detected. IP: {IP}, Path: {Path}, Route: {Key}, Value: {Value}",
                    context.Connection.RemoteIpAddress,
                    context.Request.Path,
                    routeValue.Key,
                    value
                );

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Invalid request detected.",
                    error = "VALIDATION_ERROR"
                });
                return;
            }
        }

        await _next(context);
    }
}
