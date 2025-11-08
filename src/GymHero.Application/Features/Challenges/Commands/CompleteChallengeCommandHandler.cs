using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Challenges.Commands;

public class CompleteChallengeCommandHandler : IRequestHandler<CompleteChallengeCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public CompleteChallengeCommandHandler(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<Unit> Handle(CompleteChallengeCommand request, CancellationToken cancellationToken)
    {
        // Find the challenge
        var challenge = await _context.Challenges
            .Include(c => c.Progresses)
            .FirstOrDefaultAsync(c => c.Id == request.ChallengeId, cancellationToken);

        if (challenge == null)
        {
            throw new KeyNotFoundException("Desafio não encontrado");
        }

        // Check if user is a participant in this challenge
        var progress = challenge.Progresses?.FirstOrDefault(p => p.ParticipantId == request.UserId);
        if (progress == null)
        {
            throw new UnauthorizedAccessException("Você não é um participante deste desafio");
        }

        // Update challenge status to Completed
        challenge.Status = "Completed";

        // Update progress to show completion
        progress.CurrentValue = challenge.TargetValue;
        progress.LastUpdate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Send notification to user who completed the challenge
        await _notificationService.CreateChallengeCompletedNotificationAsync(
            request.UserId,
            challenge.Id,
            challenge.Title,
            cancellationToken);

        return Unit.Value;
    }
}
