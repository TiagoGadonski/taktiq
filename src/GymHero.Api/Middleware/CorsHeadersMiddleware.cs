namespace GymHero.Api.Middleware;

/// <summary>
/// Middleware that ensures CORS headers are present in all responses,
/// even when exceptions occur or other middleware modifies the response.
/// </summary>
public class CorsHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorsHeadersMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public CorsHeadersMiddleware(
        RequestDelegate next,
        ILogger<CorsHeadersMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get the Origin header from the request
        var origin = context.Request.Headers["Origin"].ToString();

        // Define allowed origins based on environment
        var allowedOrigins = _environment.IsDevelopment()
            ? new[] { "http://localhost:3000", "http://localhost:3001", "http://localhost:3002", "http://127.0.0.1:3000" }
            : new[] {
                "https://taktiq.app",
                "https://www.taktiq.app",
                "https://taktiq-web-frontend.azurewebsites.net",
                "https://taktiq-web-frontend-fzetgjhvhqbpdtc4.brazilsouth-01.azurewebsites.net"
            };

        // Check if the origin is allowed
        var isAllowedOrigin = !string.IsNullOrEmpty(origin) && allowedOrigins.Contains(origin);

        if (isAllowedOrigin)
        {
            // Set CORS headers before calling next middleware
            context.Response.OnStarting(() =>
            {
                // Only add headers if they don't already exist
                if (!context.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
                {
                    context.Response.Headers["Access-Control-Allow-Origin"] = origin;
                    _logger.LogDebug("Added Access-Control-Allow-Origin header: {Origin}", origin);
                }
                if (!context.Response.Headers.ContainsKey("Access-Control-Allow-Credentials"))
                {
                    context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
                }
                if (!context.Response.Headers.ContainsKey("Access-Control-Allow-Headers"))
                {
                    context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization";
                }
                if (!context.Response.Headers.ContainsKey("Access-Control-Allow-Methods"))
                {
                    context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, PATCH, OPTIONS";
                }

                return Task.CompletedTask;
            });
        }

        // Handle preflight requests
        if (context.Request.Method == "OPTIONS" && isAllowedOrigin)
        {
            context.Response.StatusCode = 204;
            return;
        }

        await _next(context);
    }
}
