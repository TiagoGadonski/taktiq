using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Sessions.Queries;

public class GetCurrentSessionQueryHandler : IRequestHandler<GetCurrentSessionQuery, WorkoutSessionDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCurrentSessionQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<WorkoutSessionDto?> Handle(GetCurrentSessionQuery request, CancellationToken cancellationToken)
    {
        // Encontra a sessão ativa mais recente do usuário (sessão sem CompletedAt)
        // Nota: WorkoutSession não tem OwnerId/UserId direto, então precisamos buscar através do WorkoutPlan
        var activeSession = await _context.WorkoutSessions
            .Include(s => s.WorkoutPlan)
                .ThenInclude(p => p.Workouts)
                    .ThenInclude(w => w.Exercises)
                        .ThenInclude(e => e.Exercise)
            .Include(s => s.Sets)
                .ThenInclude(set => set.Exercise)
            .Where(s => s.CompletedAt == null)
            .Where(s => s.WorkoutPlan == null || s.WorkoutPlan.OwnerId == request.UserId)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeSession == null)
            return null;

        return new WorkoutSessionDto
        {
            Id = activeSession.Id,
            WorkoutPlanId = activeSession.WorkoutPlanId,
            WorkoutPlan = activeSession.WorkoutPlan == null ? null : new WorkoutPlanDetailResponse
            {
                Id = activeSession.WorkoutPlan.Id,
                Name = activeSession.WorkoutPlan.Name,
                Goal = activeSession.WorkoutPlan.Goal,
                IsActive = activeSession.WorkoutPlan.IsActive,
                Workouts = activeSession.WorkoutPlan.Workouts
                    .OrderBy(w => w.Order)
                    .Select(w => new WorkoutDto
                    {
                        Id = w.Id,
                        Name = w.Name,
                        DayOfWeek = w.DayOfWeek,
                        Order = w.Order,
                        Exercises = w.Exercises
                            .OrderBy(e => e.Order)
                            .Select(e => new WorkoutExerciseDto
                            {
                                Id = e.Id,
                                ExerciseId = e.ExerciseId,
                                ExerciseName = e.Exercise?.Name ?? "",
                                Exercise = e.Exercise == null ? null : new ExerciseDto
                                {
                                    Id = e.Exercise.Id,
                                    Name = e.Exercise.Name,
                                    MuscleGroup = e.Exercise.MuscleGroup,
                                    Category = e.Exercise.Category,
                                    Equipment = e.Exercise.Equipment,
                                    Notes = e.Exercise.Notes,
                                    VideoUrl = e.Exercise.VideoUrl,
                                    ImageUrl = e.Exercise.ImageUrl
                                },
                                Order = e.Order,
                                TargetSets = e.TargetSets,
                                TargetReps = e.TargetReps,
                                TargetLoad = e.TargetLoad
                            }).ToList()
                    }).ToList(),
                Exercises = activeSession.WorkoutPlan.Workouts
                    .SelectMany(w => w.Exercises)
                    .OrderBy(e => e.Order)
                    .Select(e => new WorkoutExerciseDto
                    {
                        Id = e.Id,
                        ExerciseId = e.ExerciseId,
                        ExerciseName = e.Exercise?.Name ?? "",
                        Exercise = e.Exercise == null ? null : new ExerciseDto
                        {
                            Id = e.Exercise.Id,
                            Name = e.Exercise.Name,
                            MuscleGroup = e.Exercise.MuscleGroup,
                            Category = e.Exercise.Category,
                            Equipment = e.Exercise.Equipment,
                            Notes = e.Exercise.Notes,
                            VideoUrl = e.Exercise.VideoUrl,
                            ImageUrl = e.Exercise.ImageUrl
                        },
                        Order = e.Order,
                        TargetSets = e.TargetSets,
                        TargetReps = e.TargetReps,
                        TargetLoad = e.TargetLoad
                    }).ToList()
            },
            StartedAt = activeSession.StartedAt,
            CompletedAt = activeSession.CompletedAt,
            Sets = activeSession.Sets.Select(set => new WorkoutSetDto
            {
                Id = set.Id,
                ExerciseId = set.ExerciseId,
                ExerciseName = set.Exercise?.Name ?? "",
                SetNumber = set.SetNumber,
                Reps = set.Reps,
                Load = set.Load,
                Rpe = set.Rpe
            }).ToList()
        };
    }
}
