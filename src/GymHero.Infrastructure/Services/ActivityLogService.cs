using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Infrastructure.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly IApplicationDbContext _context;

    public ActivityLogService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogActivityAsync(
        Guid? userId,
        string action,
        string httpMethod,
        string endpoint,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null,
        int? statusCode = null,
        long? responseTimeMs = null,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var log = new UserActivityLog
            {
                UserId = userId,
                Action = action,
                HttpMethod = httpMethod,
                Endpoint = endpoint,
                Details = details,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                StatusCode = statusCode,
                ResponseTimeMs = responseTimeMs,
                ErrorMessage = errorMessage,
                Timestamp = DateTime.UtcNow
            };

            await _context.UserActivityLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Swallow silently - logging failures must not break the caller
        }
    }

    public async Task<IEnumerable<UserActivityLog>> GetLogsAsync(
        int page = 1,
        int pageSize = 50,
        Guid? userId = null,
        string? action = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.UserActivityLogs
            .Include(log => log.User)
            .AsNoTracking()
            .AsQueryable();

        // Apply filters
        if (userId.HasValue)
        {
            query = query.Where(log => log.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(log => log.Action.Contains(action));
        }

        if (startDate.HasValue)
        {
            query = query.Where(log => log.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(log => log.Timestamp <= endDate.Value);
        }

        // Order by most recent first
        query = query.OrderByDescending(log => log.Timestamp);

        // Apply pagination
        var skip = (page - 1) * pageSize;
        query = query.Skip(skip).Take(pageSize);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<int> GetLogCountAsync(
        Guid? userId = null,
        string? action = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.UserActivityLogs.AsQueryable();

        // Apply filters
        if (userId.HasValue)
        {
            query = query.Where(log => log.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(log => log.Action.Contains(action));
        }

        if (startDate.HasValue)
        {
            query = query.Where(log => log.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(log => log.Timestamp <= endDate.Value);
        }

        return await query.CountAsync(cancellationToken);
    }
}
