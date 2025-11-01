using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Challenges.Commands;

public class CompleteChallengeCommandHandler : IRequestHandler<CompleteChallengeCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public CompleteChallengeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
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
        challenge.Status = ChallengeStatus.Completed;

        // Update progress to show completion
        progress.CurrentValue = challenge.TargetValue;
        progress.LastUpdate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
