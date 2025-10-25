using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public class CloneWorkoutPlanCommandHandler : IRequestHandler<CloneWorkoutPlanCommand, WorkoutPlanResponse>
{
    private readonly IApplicationDbContext _context;
    public CloneWorkoutPlanCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<WorkoutPlanResponse> Handle(CloneWorkoutPlanCommand request, CancellationToken cancellationToken)
    {
        // 1. Encontrar o plano original, incluindo os seus workouts e exercícios.
        var originalPlan = await _context.WorkoutPlans
            .AsNoTracking() // Usamos AsNoTracking porque não vamos modificar o original.
            .Include(p => p.Workouts)
                .ThenInclude(w => w.Exercises)
            .FirstOrDefaultAsync(p => p.Id == request.OriginalPlanId, cancellationToken);

        if (originalPlan is null)
        {
            throw new NotFoundException("Plano de treino original não encontrado.");
        }

        // 2. Criar a nova entidade WorkoutPlan (a cópia)
        var clonedPlan = new WorkoutPlan
        {
            Name = $"{originalPlan.Name} (Cópia)",
            Goal = originalPlan.Goal,
            Description = originalPlan.Description,
            Duration = originalPlan.Duration,
            OwnerId = request.NewOwnerId // O dono agora é o utilizador que clonou
        };

        // 3. Criar cópias de todos os Workouts e seus exercícios
        foreach (var originalWorkout in originalPlan.Workouts)
        {
            var clonedWorkout = new Workout
            {
                Name = originalWorkout.Name,
                DayOfWeek = originalWorkout.DayOfWeek,
                Order = originalWorkout.Order
            };

            foreach (var originalExercise in originalWorkout.Exercises)
            {
                clonedWorkout.Exercises.Add(new WorkoutExercise
                {
                    ExerciseId = originalExercise.ExerciseId,
                    Order = originalExercise.Order,
                    TargetSets = originalExercise.TargetSets,
                    TargetReps = originalExercise.TargetReps,
                    TargetLoad = originalExercise.TargetLoad,
                    TargetRepsRange = originalExercise.TargetRepsRange,
                    TargetRpe = originalExercise.TargetRpe,
                    RestSeconds = originalExercise.RestSeconds,
                    Notes = originalExercise.Notes
                });
            }

            clonedPlan.Workouts.Add(clonedWorkout);
        }

        // 4. Adicionar o novo plano completo (com os exercícios) à base de dados
        await _context.WorkoutPlans.AddAsync(clonedPlan, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // 5. Retornar os dados do plano recém-criado
        return new WorkoutPlanResponse(clonedPlan.Id, clonedPlan.OwnerId, clonedPlan.Name, clonedPlan.Goal, clonedPlan.IsActive, clonedPlan.CreatedAt);
    }
}