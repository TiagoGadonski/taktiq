using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Sessions.Commands;

public class CancelWorkoutSessionCommandHandler : IRequestHandler<CancelWorkoutSessionCommand>
{
    private readonly IApplicationDbContext _context;

    public CancelWorkoutSessionCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(CancelWorkoutSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.WorkoutSessions
            .Include(s => s.Sets)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session is null)
        {
            throw new NotFoundException("Workout Session not found.");
        }

        // Validate ownership
        if (session.OwnerId != request.OwnerId)
        {
            throw new NotFoundException("Workout Session not found.");
        }

        // Don't allow canceling completed sessions
        if (session.CompletedAt.HasValue)
        {
            throw new InvalidOperationException("Cannot cancel a completed workout session.");
        }

        // Remove all sets associated with this session
        _context.WorkoutSets.RemoveRange(session.Sets);

        // Remove the session itself
        _context.WorkoutSessions.Remove(session);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
