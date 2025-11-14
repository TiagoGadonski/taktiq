using GymHero.Application.Common.Interfaces;
using DomainVisibility = GymHero.Domain.Enums.VisibilityLevel;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.WorkoutPlans.Queries;

public class GetPublicWorkoutPlanByIdQueryHandler : IRequestHandler<GetPublicWorkoutPlanByIdQuery, WorkoutPlanDetailResponse?>
{
    private readonly IApplicationDbContext _context;

    public GetPublicWorkoutPlanByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<WorkoutPlanDetailResponse?> Handle(GetPublicWorkoutPlanByIdQuery request, CancellationToken cancellationToken)
    {
        // First check if plan exists and is public
        var planEntity = await _context.WorkoutPlans
            .Include(p => p.Workouts)
                .ThenInclude(w => w.Exercises)
                    .ThenInclude(we => we.Exercise)
            .FirstOrDefaultAsync(p => p.Id == request.PlanId && p.IsPublic && p.VisibilityLevel == DomainVisibility.Public, cancellationToken);

        if (planEntity == null)
            return null;

        // Increment view count (using a separate tracked context query)
        var planToUpdate = await _context.WorkoutPlans.FindAsync(new object[] { request.PlanId }, cancellationToken);
        if (planToUpdate != null)
        {
            planToUpdate.ViewCount++;
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Return the plan details with workouts grouped properly
        var plan = new WorkoutPlanDetailResponse
        {
            Id = planEntity.Id,
            Name = planEntity.Name,
            Goal = planEntity.Goal,
            IsActive = planEntity.IsActive,
            Duration = planEntity.Duration,
            StartDate = planEntity.StartDate,
            ExpirationDate = planEntity.ExpirationDate,

            // Populate Workouts property with nested exercises
            Workouts = planEntity.Workouts
                .OrderBy(w => w.Order)
                .Select(w => new WorkoutDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    DayOfWeek = w.DayOfWeek,
                    Order = w.Order,
                    Exercises = w.Exercises
                        .OrderBy(e => e.Order)
                        .Select(we => new WorkoutExerciseDto
                        {
                            Id = we.Id,
                            ExerciseId = we.ExerciseId,
                            ExerciseName = we.Exercise.Name,
                            Order = we.Order,
                            TargetSets = we.TargetSets,
                            TargetReps = we.TargetReps,
                            TargetLoad = we.TargetLoad,
                            Exercise = new ExerciseDto
                            {
                                Id = we.Exercise.Id,
                                Name = we.Exercise.Name,
                                MuscleGroup = we.Exercise.MuscleGroup,
                                Equipment = we.Exercise.Equipment,
                                Category = we.Exercise.Category
                            }
                        }).ToList()
                }).ToList(),

            // Also keep flat exercises list for backward compatibility
            Exercises = planEntity.Workouts
                .SelectMany(w => w.Exercises)
                .OrderBy(e => e.Order)
                .Select(we => new WorkoutExerciseDto
                {
                    Id = we.Id,
                    ExerciseId = we.ExerciseId,
                    ExerciseName = we.Exercise.Name,
                    Order = we.Order,
                    TargetSets = we.TargetSets,
                    TargetReps = we.TargetReps,
                    TargetLoad = we.TargetLoad
                }).ToList()
        };

        return plan;
    }
}