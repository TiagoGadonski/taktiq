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
        // Use Select projection to load only needed fields and avoid N+1 queries
        var activeSession = await _context.WorkoutSessions
            .AsNoTracking()
            .Where(s => s.OwnerId == request.UserId && s.CompletedAt == null)
            .OrderByDescending(s => s.StartedAt)
            .Select(s => new WorkoutSessionDto
            {
                Id = s.Id,
                WorkoutPlanId = s.WorkoutPlanId,
                StartedAt = s.StartedAt,
                CompletedAt = s.CompletedAt,
                Notes = s.Notes,
                WorkoutPlan = s.WorkoutPlan == null ? null : new WorkoutPlanDetailResponse
                {
                    Id = s.WorkoutPlan.Id,
                    Name = s.WorkoutPlan.Name,
                    Goal = s.WorkoutPlan.Goal,
                    IsActive = s.WorkoutPlan.IsActive,
                    Workouts = s.WorkoutPlan.Workouts
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
                                    ExerciseName = e.Exercise != null ? e.Exercise.Name : "",
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
                    Exercises = s.WorkoutPlan.Workouts
                        .SelectMany(w => w.Exercises)
                        .OrderBy(e => e.Order)
                        .Select(e => new WorkoutExerciseDto
                        {
                            Id = e.Id,
                            ExerciseId = e.ExerciseId,
                            ExerciseName = e.Exercise != null ? e.Exercise.Name : "",
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
                Sets = s.Sets.Select(set => new WorkoutSetDto
                {
                    Id = set.Id,
                    ExerciseId = set.ExerciseId,
                    ExerciseName = set.Exercise != null ? set.Exercise.Name : "",
                    SetNumber = set.SetNumber,
                    Reps = set.Reps,
                    Load = set.Load,
                    Rpe = set.Rpe,
                    IsAddedDuringSession = set.IsAddedDuringSession
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return activeSession;
    }
}
