using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public class DeleteWorkoutPlanCommandHandler : IRequestHandler<DeleteWorkoutPlanCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteWorkoutPlanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteWorkoutPlanCommand request, CancellationToken cancellationToken){

        // 1. Busca o plano de treino garantindo que ele pertence ao usuário logado.
        var workoutPlan = await _context.WorkoutPlans
            .FirstOrDefaultAsync(wp => wp.Id == request.Id && wp.OwnerId == request.OwnerId, cancellationToken);

        // 2. Se não encontrar, lança a mesma exceção de antes.
        if (workoutPlan is null)
        {
            throw new NotFoundException("Workout Plan not found.");
        }

        // 3. Remove a referência do plano de treino das sessões relacionadas (preserva o histórico)
        var relatedSessions = await _context.WorkoutSessions
            .Where(ws => ws.WorkoutPlanId == request.Id)
            .ToListAsync(cancellationToken);

        foreach (var session in relatedSessions)
        {
            session.WorkoutPlanId = null;
        }

        // 4. Marca a entidade para ser removida.
        _context.WorkoutPlans.Remove(workoutPlan);

        // 5. Salva as mudanças, efetivando a exclusão no banco de dados.
        await _context.SaveChangesAsync(cancellationToken);
    }
}