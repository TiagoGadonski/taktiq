using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public class ReplaceExerciseInWorkoutCommandHandler : IRequestHandler<ReplaceExerciseInWorkoutCommand>
{
    private readonly IApplicationDbContext _context;

    public ReplaceExerciseInWorkoutCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ReplaceExerciseInWorkoutCommand request, CancellationToken cancellationToken)
    {
        // Find the workout exercise and verify ownership
        var workoutExercise = await _context.WorkoutExercises
            .Include(we => we.Workout)
                .ThenInclude(w => w.Plan)
                    .ThenInclude(p => p.Owner)
            .FirstOrDefaultAsync(we => we.Id == request.WorkoutExerciseId, cancellationToken);

        if (workoutExercise == null)
        {
            throw new NotFoundException($"Workout exercise with ID {request.WorkoutExerciseId} not found");
        }

        // Check if user has permission to modify this workout's plan
        // Permission is granted if:
        // - User is the owner of the plan, OR
        // - User is the Personal Trainer of the plan's owner
        bool hasPermission = workoutExercise.Workout.Plan.OwnerId == request.OwnerId || // User is the owner
                           workoutExercise.Workout.Plan.Owner?.PersonalTrainerId == request.OwnerId; // User is the PT

        if (!hasPermission)
        {
            throw new NotFoundException($"Workout exercise with ID {request.WorkoutExerciseId} not found");
        }

        // Verify the new exercise exists
        var newExercise = await _context.Exercises
            .FindAsync(new object[] { request.NewExerciseId }, cancellationToken);

        if (newExercise == null)
        {
            throw new NotFoundException($"Exercise with ID {request.NewExerciseId} not found");
        }

        // Replace the exercise
        workoutExercise.ExerciseId = request.NewExerciseId;

        // Update sets/reps/load if provided
        if (request.TargetSets.HasValue)
        {
            workoutExercise.TargetSets = request.TargetSets.Value;
        }

        if (request.TargetReps.HasValue)
        {
            workoutExercise.TargetReps = request.TargetReps.Value;
        }

        if (request.TargetLoad.HasValue)
        {
            workoutExercise.TargetLoad = request.TargetLoad.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
