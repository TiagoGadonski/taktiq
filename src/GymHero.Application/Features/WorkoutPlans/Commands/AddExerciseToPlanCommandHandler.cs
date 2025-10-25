using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public class AddExerciseToPlanCommandHandler : IRequestHandler<AddExerciseToPlanCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddExerciseToPlanCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(AddExerciseToPlanCommand request, CancellationToken cancellationToken)
    {
        // Validação 1: O plano de treino existe e pertence ao usuário?
        var plan = await _context.WorkoutPlans
            .Include(p => p.Workouts)
                .ThenInclude(w => w.Exercises)
            .FirstOrDefaultAsync(p => p.Id == request.WorkoutPlanId && p.OwnerId == request.OwnerId, cancellationToken);

        if (plan is null)
        {
            throw new NotFoundException("Workout Plan not found.");
        }

        // Validação 2: O exercício que se quer adicionar realmente existe no nosso catálogo?
        var exerciseExists = await _context.Exercises
            .AnyAsync(e => e.Id == request.ExerciseId, cancellationToken);

        if (!exerciseExists)
        {
            throw new NotFoundException("Exercise to be added not found.");
        }

        // BRIDGE SOLUTION: Encontrar ou criar um treino padrão para adicionar os exercícios
        var defaultWorkout = plan.Workouts.FirstOrDefault();
        if (defaultWorkout is null)
        {
            defaultWorkout = new Workout
            {
                PlanId = plan.Id,
                Name = "Treino Completo",
                Order = 1,
                DayOfWeek = null
            };
            // Add to DbSet directly and mark as Added
            _context.Workouts.Add(defaultWorkout);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Se tudo estiver OK, criamos a entidade de associação
        var workoutExercise = new WorkoutExercise
        {
            WorkoutId = defaultWorkout.Id,
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