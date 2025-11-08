using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public class DuplicateWorkoutPlanCommandHandler : IRequestHandler<DuplicateWorkoutPlanCommand, WorkoutPlanResponse>
{
    private readonly IApplicationDbContext _context;

    public DuplicateWorkoutPlanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WorkoutPlanResponse> Handle(DuplicateWorkoutPlanCommand request, CancellationToken cancellationToken)
    {
        // Find the original plan with all its workouts and exercises
        var originalPlan = await _context.WorkoutPlans
            .Include(p => p.Workouts)
                .ThenInclude(w => w.Exercises)
            .FirstOrDefaultAsync(p => p.Id == request.PlanId && p.OwnerId == request.OwnerId, cancellationToken);

        if (originalPlan == null)
            throw new InvalidOperationException("Plano não encontrado");

        // Calculate dates
        var startDate = DateTime.UtcNow;
        var expirationDate = startDate.AddDays(request.Duration * 7);

        // Create new plan
        var newPlan = new WorkoutPlan
        {
            Name = $"{originalPlan.Name} (Cópia)",
            Goal = originalPlan.Goal,
            OwnerId = request.OwnerId,
            Duration = request.Duration,
            StartDate = startDate,
            ExpirationDate = expirationDate,
            IsActive = false
        };

        // Copy workouts and exercises
        foreach (var originalWorkout in originalPlan.Workouts)
        {
            var newWorkout = new Workout
            {
                Name = originalWorkout.Name,
                DayOfWeek = originalWorkout.DayOfWeek,
                Order = originalWorkout.Order,
                PlanId = newPlan.Id
            };

            // Copy exercises
            foreach (var originalExercise in originalWorkout.Exercises)
            {
                var newExercise = new WorkoutExercise
                {
                    ExerciseId = originalExercise.ExerciseId,
                    Order = originalExercise.Order,
                    TargetSets = originalExercise.TargetSets,
                    TargetReps = originalExercise.TargetReps,
                    TargetLoad = originalExercise.TargetLoad
                };

                newWorkout.Exercises.Add(newExercise);
            }

            newPlan.Workouts.Add(newWorkout);
        }

        // Save the new plan
        await _context.WorkoutPlans.AddAsync(newPlan, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Return response
        return new WorkoutPlanResponse(
            newPlan.Id,
            newPlan.OwnerId,
            newPlan.Name,
            newPlan.Goal,
            newPlan.IsActive,
            newPlan.CreatedAt,
            newPlan.Duration,
            newPlan.StartDate,
            newPlan.ExpirationDate
        );
    }
}
