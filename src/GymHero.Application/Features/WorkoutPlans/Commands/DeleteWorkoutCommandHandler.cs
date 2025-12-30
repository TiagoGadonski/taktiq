using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public class DeleteWorkoutCommandHandler : IRequestHandler<DeleteWorkoutCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteWorkoutCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteWorkoutCommand request, CancellationToken cancellationToken)
    {
        // 1. Find the workout and include its plan to verify ownership
        var workout = await _context.Workouts
            .Include(w => w.Plan)
                .ThenInclude(p => p.Owner)
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutId, cancellationToken);

        // 2. If workout doesn't exist, throw exception
        if (workout == null)
        {
            throw new NotFoundException("Workout not found.");
        }

        // 3. Verify user has permission to modify this workout's plan
        // Permission is granted if:
        // - User is the owner of the plan, OR
        // - User is the Personal Trainer of the plan's owner
        bool hasPermission = workout.Plan.OwnerId == request.OwnerId || // User is the owner
                           workout.Plan.Owner?.PersonalTrainerId == request.OwnerId; // User is the PT

        if (!hasPermission)
        {
            throw new NotFoundException("Workout not found.");
        }

        // 4. Remove the workout (cascade will handle WorkoutExercises)
        _context.Workouts.Remove(workout);

        // 5. Save changes to the database
        await _context.SaveChangesAsync(cancellationToken);
    }
}
