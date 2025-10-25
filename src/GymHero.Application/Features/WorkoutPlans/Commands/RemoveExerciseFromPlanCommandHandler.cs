using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public class RemoveExerciseFromPlanCommandHandler : IRequestHandler<RemoveExerciseFromPlanCommand>
{
    private readonly IApplicationDbContext _context;

    public RemoveExerciseFromPlanCommandHandler(IApplicationDbContext context) => _context = context;
    
    public async Task Handle(RemoveExerciseFromPlanCommand request, CancellationToken cancellationToken)
    {
        // Esta é a consulta de segurança mais importante até agora.
        // Buscamos o registro 'WorkoutExercise' que queremos deletar...
        var workoutExercise = await _context.WorkoutExercises
            // ...e usamos o 'Include' para trazer junto os dados do Workout e do Plano de Treino associado...
            .Include(we => we.Workout)
                .ThenInclude(w => w.Plan)
            // ...para que possamos validar tudo em uma única consulta ao banco:
            // 1. O ID do registro 'WorkoutExercise' bate?
            // 2. O ID do Plano de Treino pai bate? (usando a propriedade computada WorkoutPlanId)
            // 3. O Dono do Plano de Treino é o usuário que fez a requisição? (usando a propriedade computada WorkoutPlan)
            .FirstOrDefaultAsync(we =>
                we.Id == request.WorkoutExerciseId &&
                we.Workout.PlanId == request.WorkoutPlanId &&
                we.Workout.Plan.OwnerId == request.OwnerId,
                cancellationToken);

        if (workoutExercise is null)
        {
            // Se qualquer uma das condições acima falhar, não encontramos o registro e lançamos a exceção.
            throw new NotFoundException("Exercise link not found in the specified workout plan.");
        }

        _context.WorkoutExercises.Remove(workoutExercise);
        await _context.SaveChangesAsync(cancellationToken);
    }
}