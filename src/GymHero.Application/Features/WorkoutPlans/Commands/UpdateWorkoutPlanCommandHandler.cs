using GymHero.Application.Common.Exceptions; // Vamos criar esta exceção
using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public class UpdateWorkoutPlanCommandHandler : IRequestHandler<UpdateWorkoutPlanCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateWorkoutPlanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateWorkoutPlanCommand request, CancellationToken cancellationToken)
    {
        // 1. Busca o plano de treino no banco. A condição é crucial:
        // O ID do plano tem que bater E o OwnerId também.
        var workoutPlan = await _context.WorkoutPlans
            .FirstOrDefaultAsync(wp => wp.Id == request.Id && wp.OwnerId == request.OwnerId, cancellationToken);

        // 2. Se não encontrar, lança uma exceção.
        // Isso previne que um usuário edite planos de outros ou planos que não existem.
        if (workoutPlan is null)
        {
            throw new NotFoundException("Workout Plan not found.");
        }

        // 3. Atualiza as propriedades da entidade com os novos valores
        workoutPlan.Name = request.Name;
        workoutPlan.Goal = request.Goal;

        // 4. Salva as mudanças no banco de dados
        await _context.SaveChangesAsync(cancellationToken);
    }
}