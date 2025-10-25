using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public class AddExerciseToWorkoutCommandHandler : IRequestHandler<AddExerciseToWorkoutCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddExerciseToWorkoutCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddExerciseToWorkoutCommand request, CancellationToken cancellationToken)
    {
        // Verify the workout exists and belongs to the user
        var workout = await _context.Workouts
            .Include(w => w.Plan)
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutId, cancellationToken);

        if (workout == null)
        {
            throw new NotFoundException($"Workout with ID {request.WorkoutId} not found");
        }

        if (workout.Plan.OwnerId != request.OwnerId)
        {
            throw new NotFoundException($"Workout with ID {request.WorkoutId} not found");
        }

        // Verify the exercise exists
        var exercise = await _context.Exercises
            .FindAsync(new object[] { request.ExerciseId }, cancellationToken);

        if (exercise == null)
        {
            throw new NotFoundException($"Exercise with ID {request.ExerciseId} not found");
        }

        // Create the workout exercise
        var workoutExercise = new WorkoutExercise
        {
            WorkoutId = request.WorkoutId,
            ExerciseId = request.ExerciseId,
            Order = request.Order,
            TargetSets = request.TargetSets,
            TargetReps = request.TargetReps,
            TargetLoad = request.TargetLoad
        };

        _context.WorkoutExercises.Add(workoutExercise);
        await _context.SaveChangesAsync(cancellationToken);

        return workoutExercise.Id;
    }
}
