using System.Collections.Concurrent;
using System.Security.Claims;

namespace GymHero.Api.Middleware;

/// <summary>
/// Rate limiting middleware specifically for chat endpoints to prevent spam and abuse
/// </summary>
public class ChatRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ChatRateLimitingMiddleware> _logger;

    // Simple in-memory rate limiting - in production, use Redis for distributed systems
    private static readonly ConcurrentDictionary<string, UserRateLimit> _rateLimits = new();

    // Rate limit configuration
    private const int MaxMessagesPerMinute = 30;
    private const int MaxTypingIndicatorsPerMinute = 20;
    private const int CleanupIntervalMinutes = 5;

    private static DateTime _lastCleanup = DateTime.UtcNow;

    public ChatRateLimitingMiddleware(RequestDelegate next, ILogger<ChatRateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only apply to chat endpoints
        if (!context.Request.Path.StartsWithSegments("/api/chat"))
        {
            await _next(context);
            return;
        }

        var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            await _next(context);
            return;
        }

        // Periodic cleanup of old entries
        if ((DateTime.UtcNow - _lastCleanup).TotalMinutes > CleanupIntervalMinutes)
        {
            CleanupOldEntries();
        }

        var userLimit = _rateLimits.GetOrAdd(userId, _ => new UserRateLimit());

        // Check rate limits based on endpoint
        var path = context.Request.Path.Value?.ToLower() ?? "";
        var method = context.Request.Method.ToUpper();

        if (method == "POST" && path.Contains("/messages") && !path.Contains("mark-read"))
        {
            // Rate limit message sending
            if (!userLimit.CanSendMessage())
            {
                _logger.LogWarning("Rate limit exceeded for user {UserId} on message sending", userId);
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["Retry-After"] = "60";
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Muitas mensagens enviadas. Por favor, aguarde um momento."
                });
                return;
            }
        }

        await _next(context);
    }

    private static void CleanupOldEntries()
    {
        _lastCleanup = DateTime.UtcNow;
        var keysToRemove = new List<string>();

        foreach (var kvp in _rateLimits)
        {
            if (kvp.Value.IsExpired())
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            _rateLimits.TryRemove(key, out _);
        }
    }

    private class UserRateLimit
    {
        private readonly Queue<DateTime> _messageTimes = new();
        private readonly object _lock = new();
        private DateTime _lastActivity = DateTime.UtcNow;

        public bool CanSendMessage()
        {
            lock (_lock)
            {
                _lastActivity = DateTime.UtcNow;
                var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);

                // Remove old entries
                while (_messageTimes.Count > 0 && _messageTimes.Peek() < oneMinuteAgo)
                {
                    _messageTimes.Dequeue();
                }

                // Check if under limit
                if (_messageTimes.Count >= MaxMessagesPerMinute)
                {
                    return false;
                }

                // Add new entry
                _messageTimes.Enqueue(DateTime.UtcNow);
                return true;
            }
        }

        public bool IsExpired()
        {
            return (DateTime.UtcNow - _lastActivity).TotalMinutes > CleanupIntervalMinutes;
        }
    }
}
