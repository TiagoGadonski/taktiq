using GymHero.Application.Common.Interfaces;
using GymHero.Application.Features.Sessions.Events;
using GymHero.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Adicionar para logs

namespace GymHero.Application.Features.Badges.EventHandlers;

public class WorkoutCompletedBadgeHandler : INotificationHandler<WorkoutSessionCompletedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<WorkoutCompletedBadgeHandler> _logger;

    public WorkoutCompletedBadgeHandler(IApplicationDbContext context, ILogger<WorkoutCompletedBadgeHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(WorkoutSessionCompletedEvent notification, CancellationToken cancellationToken)
    {
        var userId = notification.Session.WorkoutPlan.OwnerId;

        // 1. Obter todas as definições de medalhas que são acionadas pela conclusão de um treino.
        var badgeDefinitions = await _context.BadgeDefinitions
            .Where(def => def.TriggerType == "WORKOUTS_COMPLETED")
            .ToListAsync(cancellationToken);

        if (!badgeDefinitions.Any()) return; // Sem regras para processar

        // 2. Obter os códigos de todas as medalhas que o utilizador já possui para evitar verificações desnecessárias.
        var userBadgeCodes = await _context.Badges
            .Where(b => b.OwnerId == userId)
            .Select(b => b.Code)
            .ToListAsync(cancellationToken);

        // 3. Obter o dado relevante para a regra (neste caso, a contagem de treinos)
        var completedCount = await _context.WorkoutSessions
            .CountAsync(s => s.WorkoutPlan.OwnerId == userId && s.CompletedAt != null, cancellationToken);
            
        _logger.LogInformation("User {UserId} has {Count} completed workouts. Checking for new badges...", userId, completedCount);

        // 4. Iterar sobre cada regra e verificar se deve ser concedida
        foreach (var definition in badgeDefinitions)
        {
            // Se o utilizador já tem esta medalha, ou se ainda não atingiu o objetivo, passamos à próxima.
            if (userBadgeCodes.Contains(definition.Code) || completedCount < definition.ThresholdValue)
            {
                continue;
            }

            // Conceder a nova medalha!
            _logger.LogInformation("Awarding badge {BadgeCode} to user {UserId}", definition.Code, userId);
            var newBadge = new Badge
            {
                OwnerId = userId,
                Code = definition.Code,
                Title = definition.Title,
                Description = definition.Description,
                EarnedAt = DateTime.UtcNow
            };
            await _context.Badges.AddAsync(newBadge, cancellationToken);
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }
}