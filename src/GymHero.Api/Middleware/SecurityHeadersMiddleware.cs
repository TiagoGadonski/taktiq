namespace GymHero.Api.Middleware;

/// <summary>
/// Middleware to add security headers to all responses
/// Protects against common web vulnerabilities
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // X-Content-Type-Options: Prevents MIME type sniffing
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        // X-Frame-Options: Prevents clickjacking attacks
        context.Response.Headers["X-Frame-Options"] = "DENY";

        // X-XSS-Protection: Enables XSS filter in older browsers
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

        // Referrer-Policy: Controls how much referrer information is included
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Content-Security-Policy: Helps prevent XSS and data injection attacks
        // Note: Adjust this policy based on your specific needs
        var csp = "default-src 'self'; " +
                  "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                  "style-src 'self' 'unsafe-inline'; " +
                  "img-src 'self' data: https:; " +
                  "font-src 'self' data:; " +
                  "connect-src 'self' https:; " +
                  "frame-ancestors 'none';";
        context.Response.Headers["Content-Security-Policy"] = csp;

        // Permissions-Policy: Controls which browser features can be used
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

        await _next(context);
    }
}
