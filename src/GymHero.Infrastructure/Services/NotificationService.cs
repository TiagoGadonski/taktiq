using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using System.Text.Json;

namespace GymHero.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;

    public NotificationService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateNotificationAsync(
        Guid userId,
        string type,
        string title,
        string message,
        string? data = null,
        string? actionUrl = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            Data = data,
            ActionUrl = actionUrl,
            IsRead = false
        };

        await _context.Notifications.AddAsync(notification, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateFriendRequestNotificationAsync(
        Guid recipientUserId,
        Guid requesterId,
        string requesterName,
        CancellationToken cancellationToken = default)
    {
        var data = JsonSerializer.Serialize(new { RequesterId = requesterId });

        await CreateNotificationAsync(
            recipientUserId,
            "FriendRequest",
            "Nova Solicitação de Amizade",
            $"{requesterName} quer ser seu amigo!",
            data,
            "/friends",
            cancellationToken);
    }

    public async Task CreateFriendRequestAcceptedNotificationAsync(
        Guid requesterUserId,
        Guid acceptorId,
        string acceptorName,
        CancellationToken cancellationToken = default)
    {
        var data = JsonSerializer.Serialize(new { AcceptorId = acceptorId });

        await CreateNotificationAsync(
            requesterUserId,
            "FriendRequestAccepted",
            "Solicitação Aceita!",
            $"{acceptorName} aceitou sua solicitação de amizade!",
            data,
            "/friends",
            cancellationToken);
    }

    public async Task CreatePlanSharedNotificationAsync(
        Guid recipientUserId,
        Guid planId,
        string planName,
        string sharerName,
        CancellationToken cancellationToken = default)
    {
        var data = JsonSerializer.Serialize(new { PlanId = planId });

        await CreateNotificationAsync(
            recipientUserId,
            "PlanShared",
            "Plano Compartilhado",
            $"{sharerName} compartilhou o plano \"{planName}\" com você!",
            data,
            $"/plans/{planId}",
            cancellationToken);
    }

    public async Task CreateChallengeCompletedNotificationAsync(
        Guid userId,
        Guid challengeId,
        string challengeTitle,
        CancellationToken cancellationToken = default)
    {
        var data = JsonSerializer.Serialize(new { ChallengeId = challengeId });

        await CreateNotificationAsync(
            userId,
            "ChallengeCompleted",
            "Desafio Concluído! 🎉",
            $"Parabéns! Você completou o desafio \"{challengeTitle}\"!",
            data,
            "/challenges",
            cancellationToken);
    }

    public async Task CreatePlanExpiringNotificationAsync(
        Guid userId,
        Guid planId,
        string planName,
        int daysRemaining,
        CancellationToken cancellationToken = default)
    {
        var data = JsonSerializer.Serialize(new { PlanId = planId, DaysRemaining = daysRemaining });

        var message = daysRemaining == 0
            ? $"Seu plano \"{planName}\" expira hoje!"
            : $"Seu plano \"{planName}\" expira em {daysRemaining} dia(s)!";

        await CreateNotificationAsync(
            userId,
            "PlanExpiring",
            "Plano Próximo do Fim",
            message,
            data,
            $"/plans/{planId}",
            cancellationToken);
    }

    public async Task CreatePlanDeletedNotificationAsync(
        Guid trainerId,
        Guid studentId,
        string studentName,
        string planName,
        CancellationToken cancellationToken = default)
    {
        var data = JsonSerializer.Serialize(new { StudentId = studentId });

        await CreateNotificationAsync(
            trainerId,
            "PlanDeleted",
            "Plano Excluído",
            $"{studentName} excluiu o plano \"{planName}\"",
            data,
            "/instructor",
            cancellationToken);
    }

    public async Task CreatePTRequestNotificationAsync(
        Guid studentId,
        Guid trainerId,
        string trainerName,
        string? message,
        CancellationToken cancellationToken = default)
    {
        var data = JsonSerializer.Serialize(new { TrainerId = trainerId });

        var messageText = string.IsNullOrEmpty(message)
            ? $"{trainerName} quer ser seu Personal Trainer!"
            : $"{trainerName} quer ser seu Personal Trainer: \"{message}\"";

        await CreateNotificationAsync(
            studentId,
            "PTRequest",
            "Solicitação de Personal Trainer",
            messageText,
            data,
            "/pt-requests",
            cancellationToken);
    }

    public async Task CreatePTRequestAcceptedNotificationAsync(
        Guid trainerId,
        Guid studentId,
        string studentName,
        CancellationToken cancellationToken = default)
    {
        var data = JsonSerializer.Serialize(new { StudentId = studentId });

        await CreateNotificationAsync(
            trainerId,
            "PTRequestAccepted",
            "Solicitação Aceita!",
            $"{studentName} aceitou sua solicitação para ser Personal Trainer!",
            data,
            "/instructor",
            cancellationToken);
    }

    public async Task CreatePTRequestRejectedNotificationAsync(
        Guid trainerId,
        Guid studentId,
        string studentName,
        CancellationToken cancellationToken = default)
    {
        var data = JsonSerializer.Serialize(new { StudentId = studentId });

        await CreateNotificationAsync(
            trainerId,
            "PTRequestRejected",
            "Solicitação Recusada",
            $"{studentName} recusou sua solicitação para ser Personal Trainer.",
            data,
            "/instructor",
            cancellationToken);
    }
}
