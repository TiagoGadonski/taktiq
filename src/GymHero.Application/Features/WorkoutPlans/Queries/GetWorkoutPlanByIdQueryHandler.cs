using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs; // Vamos criar um novo DTO
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace GymHero.Application.Features.WorkoutPlans.Queries;

public record GetWorkoutPlanByIdQuery(Guid PlanId, Guid OwnerId) : IRequest<WorkoutPlanDetailResponse?>;

public class GetWorkoutPlanByIdQueryHandler : IRequestHandler<GetWorkoutPlanByIdQuery, WorkoutPlanDetailResponse?>
{
    private readonly IApplicationDbContext _context;
    public GetWorkoutPlanByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<WorkoutPlanDetailResponse?> Handle(GetWorkoutPlanByIdQuery request, CancellationToken cancellationToken)
    {
        // Fetch plan and check permission
        var plan = await _context.WorkoutPlans
            .AsNoTracking()
            .Include(p => p.Owner)
            .Include(p => p.Workouts) // Carrega os Workouts (dias de treino)
                .ThenInclude(w => w.Exercises) // Carrega os exercícios de cada workout
                    .ThenInclude(we => we.Exercise) // Carrega os detalhes do exercício
            .Where(p => p.Id == request.PlanId)
            .FirstOrDefaultAsync(cancellationToken);

        if (plan == null)
            return null;

        // Check if user has permission to view this plan
        // Permission is granted if:
        // 1. User is the owner of the plan, OR
        // 2. User is the Personal Trainer of the plan's owner
        bool hasPermission = plan.OwnerId == request.OwnerId || // User is the owner
                           plan.Owner?.PersonalTrainerId == request.OwnerId; // User is the PT

        if (!hasPermission)
            return null;

        // Map workouts with their exercises
        var workouts = plan.Workouts
            .OrderBy(w => w.Order)
            .Select(w => new WorkoutDto
            {
                Id = w.Id,
                Name = w.Name,
                DayOfWeek = w.DayOfWeek,
                Order = w.Order,
                Exercises = w.Exercises
                    .OrderBy(we => we.Order)
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
            })
            .ToList();

        // Also keep flattened exercises for backward compatibility
        var allExercises = plan.Workouts
            .SelectMany(w => w.Exercises)
            .OrderBy(we => we.Order)
            .Select(we => new WorkoutExerciseDto
            {
                Id = we.Id,
                ExerciseId = we.ExerciseId,
                ExerciseName = we.Exercise.Name,
                Order = we.Order,
                TargetSets = we.TargetSets,
                TargetReps = we.TargetReps,
                TargetLoad = we.TargetLoad
            })
            .ToList();

        return new WorkoutPlanDetailResponse
        {
            Id = plan.Id,
            Name = plan.Name,
            Goal = plan.Goal,
            IsActive = plan.IsActive,
            Duration = plan.Duration,
            StartDate = plan.StartDate,
            ExpirationDate = plan.ExpirationDate,
            Workouts = workouts,
            Exercises = allExercises
        };
    }
}