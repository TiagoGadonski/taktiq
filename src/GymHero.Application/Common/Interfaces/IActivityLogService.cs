using GymHero.Domain.Entities;

namespace GymHero.Application.Common.Interfaces;

public interface IActivityLogService
{
    Task LogActivityAsync(
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
        CancellationToken cancellationToken = default);

    Task<IEnumerable<UserActivityLog>> GetLogsAsync(
        int page = 1,
        int pageSize = 50,
        Guid? userId = null,
        string? action = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    Task<int> GetLogCountAsync(
        Guid? userId = null,
        string? action = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
}
