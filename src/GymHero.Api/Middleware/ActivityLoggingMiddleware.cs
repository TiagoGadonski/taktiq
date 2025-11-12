using System.Diagnostics;
using System.Security.Claims;
using GymHero.Application.Common.Interfaces;

namespace GymHero.Api.Middleware;

public class ActivityLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ActivityLoggingMiddleware> _logger;

    public ActivityLoggingMiddleware(RequestDelegate next, ILogger<ActivityLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IActivityLogService activityLogService)
    {
        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            // Continue processing the request
            await _next(context);

            stopwatch.Stop();

            // Log the activity after the request completes
            await LogActivity(context, activityLogService, stopwatch.ElapsedMilliseconds, null);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Log the activity with error information
            await LogActivity(context, activityLogService, stopwatch.ElapsedMilliseconds, ex.Message);

            // Re-throw the exception
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task LogActivity(
        HttpContext context,
        IActivityLogService activityLogService,
        long responseTimeMs,
        string? errorMessage)
    {
        try
        {
            // Skip logging for certain endpoints to avoid noise
            var path = context.Request.Path.Value?.ToLower() ?? "";
            if (ShouldSkipLogging(path))
            {
                return;
            }

            // Get user ID if authenticated
            Guid? userId = null;
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            // Determine action from endpoint
            var action = DetermineAction(context.Request.Method, path);

            // Get client information
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            var userAgent = context.Request.Headers["User-Agent"].ToString();

            // Log the activity
            await activityLogService.LogActivityAsync(
                userId: userId,
                action: action,
                httpMethod: context.Request.Method,
                endpoint: path,
                details: null,
                ipAddress: ipAddress,
                userAgent: userAgent,
                statusCode: context.Response.StatusCode,
                responseTimeMs: responseTimeMs,
                errorMessage: errorMessage
            );
        }
        catch (Exception ex)
        {
            // Don't let logging failures break the app
            _logger.LogError(ex, "Failed to log activity");
        }
    }

    private bool ShouldSkipLogging(string path)
    {
        // Skip health checks, static files, and high-frequency endpoints
        var skipPaths = new[]
        {
            "/health",
            "/swagger",
            "/_framework",
            "/css",
            "/js",
            "/images",
            "/favicon.ico"
        };

        return skipPaths.Any(skipPath => path.StartsWith(skipPath));
    }

    private string DetermineAction(string httpMethod, string path)
    {
        // Map common endpoints to readable actions
        if (path.Contains("/auth/login"))
            return "Login";
        if (path.Contains("/auth/signup"))
            return "Signup";
        if (path.Contains("/auth/logout"))
            return "Logout";
        if (path.Contains("/plans") && httpMethod == "POST")
            return "CreatePlan";
        if (path.Contains("/plans") && httpMethod == "PUT")
            return "UpdatePlan";
        if (path.Contains("/plans") && httpMethod == "DELETE")
            return "DeletePlan";
        if (path.Contains("/workouts") && httpMethod == "POST")
            return "CreateWorkout";
        if (path.Contains("/exercises") && httpMethod == "POST")
            return "CreateExercise";
        if (path.Contains("/profile") && httpMethod == "PUT")
            return "UpdateProfile";
        if (path.Contains("/friends") && httpMethod == "POST")
            return "SendFriendRequest";
        if (path.Contains("/challenges") && httpMethod == "POST")
            return "CreateChallenge";

        // Default: use HTTP method + endpoint
        return $"{httpMethod} {path}";
    }
}
