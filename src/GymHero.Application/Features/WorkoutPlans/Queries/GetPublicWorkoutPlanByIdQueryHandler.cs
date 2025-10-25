using GymHero.Application.Common.Interfaces;
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
        var plan = await _context.WorkoutPlans
            .AsNoTracking()
            .Include(p => p.Workouts)
                .ThenInclude(w => w.Exercises)
                    .ThenInclude(we => we.Exercise)
            // A condição de segurança 'OwnerId' foi removida aqui.
            // Buscamos apenas pelo ID do plano.
            .Where(p => p.Id == request.PlanId)
            .Select(p => new WorkoutPlanDetailResponse
            {
                Id = p.Id,
                Name = p.Name,
                Goal = p.Goal,
                IsActive = p.IsActive,
                Exercises = p.Workouts
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
            })
            .FirstOrDefaultAsync(cancellationToken);

        return plan;
    }
}