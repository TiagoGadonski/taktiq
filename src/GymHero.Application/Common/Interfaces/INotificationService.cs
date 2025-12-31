namespace GymHero.Application.Common.Interfaces;

public interface INotificationService
{
    Task CreateNotificationAsync(
        Guid userId,
        string type,
        string title,
        string message,
        string? data = null,
        string? actionUrl = null,
        CancellationToken cancellationToken = default);

    Task CreateFriendRequestNotificationAsync(
        Guid recipientUserId,
        Guid requesterId,
        string requesterName,
        CancellationToken cancellationToken = default);

    Task CreateFriendRequestAcceptedNotificationAsync(
        Guid requesterUserId,
        Guid acceptorId,
        string acceptorName,
        CancellationToken cancellationToken = default);

    Task CreatePlanSharedNotificationAsync(
        Guid recipientUserId,
        Guid planId,
        string planName,
        string sharerName,
        CancellationToken cancellationToken = default);

    Task CreateChallengeCompletedNotificationAsync(
        Guid userId,
        Guid challengeId,
        string challengeTitle,
        CancellationToken cancellationToken = default);

    Task CreatePlanExpiringNotificationAsync(
        Guid userId,
        Guid planId,
        string planName,
        int daysRemaining,
        CancellationToken cancellationToken = default);

    Task CreatePlanDeletedNotificationAsync(
        Guid trainerId,
        Guid studentId,
        string studentName,
        string planName,
        CancellationToken cancellationToken = default);

    Task CreatePTRequestNotificationAsync(
        Guid studentId,
        Guid trainerId,
        string trainerName,
        string? message,
        CancellationToken cancellationToken = default);

    Task CreatePTRequestAcceptedNotificationAsync(
        Guid trainerId,
        Guid studentId,
        string studentName,
        CancellationToken cancellationToken = default);

    Task CreatePTRequestRejectedNotificationAsync(
        Guid trainerId,
        Guid studentId,
        string studentName,
        CancellationToken cancellationToken = default);
}
