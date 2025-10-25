using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Sessions.Commands;

public class LogSetCommandHandler : IRequestHandler<LogSetCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public LogSetCommandHandler(IApplicationDbContext context) => _context = context;
    
    public async Task<Guid> Handle(LogSetCommand request, CancellationToken cancellationToken)
    {
        // Validação 1: A sessão de treino existe e pertence ao usuário?
        // Precisamos usar Include para acessar os dados do WorkoutPlan e verificar o OwnerId.
        var session = await _context.WorkoutSessions
            .Include(s => s.WorkoutPlan)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session is null || session.WorkoutPlan.OwnerId != request.OwnerId)
        {
            throw new NotFoundException("Workout Session not found.");
        }

        // Validação 2: O exercício existe?
        // Note: We check if the exercise exists either in the Exercises table or as a WorkoutExercise
        // This allows logging sets for exercises that are part of the workout plan
        var exerciseExists = await _context.Exercises.AnyAsync(e => e.Id == request.ExerciseId, cancellationToken);
        if (!exerciseExists)
        {
            // Check if it's a WorkoutExercise ID instead
            var workoutExerciseExists = await _context.WorkoutExercises
                .AnyAsync(we => we.Id == request.ExerciseId, cancellationToken);
            if (!workoutExerciseExists)
            {
                throw new NotFoundException("Exercise not found.");
            }
        }

        // Se tudo estiver OK, criamos o novo registro de série
        var workoutSet = new WorkoutSet
        {
            WorkoutSessionId = request.SessionId,
            ExerciseId = request.ExerciseId,
            SetNumber = request.SetNumber,
            Reps = request.Reps,
            Load = request.Load,
            Rpe = request.Rpe,
            Completed = true // Marcamos a série como completada
        };

        await _context.WorkoutSets.AddAsync(workoutSet, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return workoutSet.Id;
    }
}