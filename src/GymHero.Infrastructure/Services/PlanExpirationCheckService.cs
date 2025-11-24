using GymHero.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GymHero.Infrastructure.Services;

public class PlanExpirationCheckService : BackgroundService
{
    private readonly ILogger<PlanExpirationCheckService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Run once per day

    public PlanExpirationCheckService(
        ILogger<PlanExpirationCheckService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Plan Expiration Check Service is starting.");

        try
        {
            // Wait a bit before first execution to allow the app to fully start
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckExpiringPlansAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when the application is shutting down
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking for expiring plans.");
                }

                // Wait for the next check interval
                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when the application is shutting down
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the application is shutting down during initial delay
            _logger.LogInformation("Plan Expiration Check Service was cancelled during startup.");
        }

        _logger.LogInformation("Plan Expiration Check Service is stopping.");
    }

    private async Task CheckExpiringPlansAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var now = DateTime.UtcNow;
        var warningThreshold = now.AddDays(7); // Warn about plans expiring in the next 7 days

        // Find active plans that are expiring soon and haven't been notified yet
        var expiringPlans = await context.WorkoutPlans
            .Where(p =>
                p.IsActive &&
                p.ExpirationDate.HasValue &&
                p.ExpirationDate.Value <= warningThreshold &&
                p.ExpirationDate.Value > now)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} plans expiring soon.", expiringPlans.Count);

        foreach (var plan in expiringPlans)
        {
            var daysRemaining = (int)(plan.ExpirationDate!.Value - now).TotalDays;

            // Only notify at specific thresholds: 7 days, 3 days, 1 day
            if (daysRemaining == 7 || daysRemaining == 3 || daysRemaining == 1)
            {
                // Check if we already sent a notification for this threshold
                var existingNotification = await context.Notifications
                    .Where(n =>
                        n.UserId == plan.OwnerId &&
                        n.Type == "PlanExpiring" &&
                        n.Data != null &&
                        n.Data.Contains($"\"planId\":\"{plan.Id}\"") &&
                        n.Data.Contains($"\"daysRemaining\":{daysRemaining}"))
                    .AnyAsync(cancellationToken);

                if (!existingNotification)
                {
                    await notificationService.CreatePlanExpiringNotificationAsync(
                        plan.OwnerId,
                        plan.Id,
                        plan.Name,
                        daysRemaining,
                        cancellationToken);

                    _logger.LogInformation(
                        "Sent expiration notification for plan {PlanId} to user {UserId}. Days remaining: {Days}",
                        plan.Id,
                        plan.OwnerId,
                        daysRemaining);
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
