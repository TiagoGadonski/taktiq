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
        // Verify the workout exists and user has permission to modify it
        var workout = await _context.Workouts
            .Include(w => w.Plan)
                .ThenInclude(p => p.Owner)
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutId, cancellationToken);

        if (workout == null)
        {
            throw new NotFoundException($"Workout with ID {request.WorkoutId} not found");
        }

        // Check if user has permission to modify this workout's plan
        // Permission is granted if:
        // 1. User is the owner of the plan, OR
        // 2. User is the Personal Trainer of the plan's owner
        bool hasPermission = workout.Plan.OwnerId == request.OwnerId || // User is the owner
                           workout.Plan.Owner?.PersonalTrainerId == request.OwnerId; // User is the PT

        if (!hasPermission)
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
