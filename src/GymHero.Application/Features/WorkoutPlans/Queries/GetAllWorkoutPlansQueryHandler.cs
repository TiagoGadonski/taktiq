using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.WorkoutPlans.Queries;

public class GetAllWorkoutPlansQueryHandler : IRequestHandler<GetAllWorkoutPlansQuery, IEnumerable<WorkoutPlanDetailResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetAllWorkoutPlansQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkoutPlanDetailResponse>> Handle(GetAllWorkoutPlansQuery request, CancellationToken cancellationToken)
    {
        // 1. Acessamos a tabela de WorkoutPlans com seus Workouts e Exercícios
        var workoutPlans = await _context.WorkoutPlans
            .Include(p => p.Workouts) // Carrega os Workouts
                .ThenInclude(w => w.Exercises) // Carrega os exercícios de cada workout
                    .ThenInclude(we => we.Exercise) // Carrega os detalhes do exercício
            // 2. Filtramos para pegar APENAS os planos que pertencem ao usuário logado
            .Where(wp => wp.OwnerId == request.OwnerId)
            // 3. Ordenamos pelos mais recentes primeiro
            .OrderByDescending(wp => wp.CreatedAt)
            // 4. Projetamos os resultados para o nosso DTO de resposta com exercícios
            .Select(wp => new WorkoutPlanDetailResponse
            {
                Id = wp.Id,
                Name = wp.Name,
                Goal = wp.Goal,
                IsActive = wp.IsActive,
                Duration = wp.Duration,
                StartDate = wp.StartDate,
                ExpirationDate = wp.ExpirationDate,
                // Include workouts with their exercises
                Workouts = wp.Workouts
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
                                Exercise = new ExerciseDto
                                {
                                    Id = we.Exercise.Id,
                                    Name = we.Exercise.Name,
                                    MuscleGroup = we.Exercise.MuscleGroup,
                                    Category = we.Exercise.Category,
                                    Equipment = we.Exercise.Equipment,
                                    Notes = we.Exercise.Notes,
                                    VideoUrl = we.Exercise.VideoUrl,
                                    ImageUrl = we.Exercise.ImageUrl
                                },
                                Order = we.Order,
                                TargetSets = we.TargetSets,
                                TargetReps = we.TargetReps,
                                TargetLoad = we.TargetLoad
                            }).ToList()
                    }).ToList(),
                // Also keep flattened exercises for backward compatibility
                Exercises = wp.Workouts
                    .SelectMany(w => w.Exercises)
                    .OrderBy(we => we.Order)
                    .Select(we => new WorkoutExerciseDto
                    {
                        Id = we.Id,
                        ExerciseId = we.ExerciseId,
                        ExerciseName = we.Exercise.Name,
                        Exercise = new ExerciseDto
                        {
                            Id = we.Exercise.Id,
                            Name = we.Exercise.Name,
                            MuscleGroup = we.Exercise.MuscleGroup,
                            Category = we.Exercise.Category,
                            Equipment = we.Exercise.Equipment,
                            Notes = we.Exercise.Notes,
                            VideoUrl = we.Exercise.VideoUrl,
                            ImageUrl = we.Exercise.ImageUrl
                        },
                        Order = we.Order,
                        TargetSets = we.TargetSets,
                        TargetReps = we.TargetReps,
                        TargetLoad = we.TargetLoad
                    }).ToList()
            })
            // 5. Executamos a query no banco de dados
            .ToListAsync(cancellationToken);

        return workoutPlans;
    }
}