using GymHero.Application.Common.Interfaces;
using GymHero.Application.Features.Sessions.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymHero.Application.Features.Challenges.EventHandlers;

public class UpdateChallengeProgressHandler : INotificationHandler<WorkoutSessionCompletedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateChallengeProgressHandler> _logger;

    public UpdateChallengeProgressHandler(IApplicationDbContext context, ILogger<UpdateChallengeProgressHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(WorkoutSessionCompletedEvent notification, CancellationToken cancellationToken)
    {
        var session = notification.Session;
        var userId = session.WorkoutPlan.OwnerId;
        var now = DateTime.UtcNow;

        // Encontrar os desafios onde o utilizador é participante e que estão ativos.
        var activeChallenges = await _context.Challenges
            .Include(c => c.Progresses) // Incluímos a coleção de progressos
            .Where(c => c.Progresses.Any(p => p.ParticipantId == userId) &&
                        c.Status == "InProgress" &&
                        c.StartDate <= now &&
                        c.EndDate >= now)
            .ToListAsync(cancellationToken);

        if (!activeChallenges.Any())
        {
            return;
        }

        _logger.LogInformation("Found {Count} active challenges for user {UserId} to update.", activeChallenges.Count, userId);

        var sessionSets = await _context.WorkoutSets
            .Where(s => s.WorkoutSessionId == session.Id)
            .ToListAsync(cancellationToken);

        double sessionTotalVolume = sessionSets
            .Where(s => s.Reps.HasValue && s.Load.HasValue)
            .Sum(s => s.Reps!.Value * s.Load!.Value);
        int sessionFrequency = 1;

        foreach (var challenge in activeChallenges)
        {
            // Encontrar o registo de progresso específico deste utilizador para este desafio.
            var userProgress = challenge.Progresses.FirstOrDefault(p => p.ParticipantId == userId);
            
            if (userProgress is null) continue;

            double valueToAdd = 0;
            switch (challenge.Type)
            {
                case "TOTAL_VOLUME":
                    valueToAdd = sessionTotalVolume;
                    break;
                case "SESSION_FREQUENCY":
                    valueToAdd = sessionFrequency;
                    break;
            }

            if (valueToAdd > 0)
            {
                userProgress.CurrentValue += valueToAdd;
                userProgress.LastUpdate = now;

                // A verificação de conclusão agora olha para o progresso total (soma de todos os participantes).
                if (challenge.Progresses.Sum(p => p.CurrentValue) >= challenge.TargetValue)
                {
                    challenge.Status = "Completed";
                    _logger.LogInformation("Collective Challenge {ChallengeId} has been completed!", challenge.Id);
                }
            }
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }
}