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

        // 1. Find the workout plan and verify user has permission
        var workoutPlan = await _context.WorkoutPlans
            .Include(wp => wp.Owner)
            .FirstOrDefaultAsync(wp => wp.Id == request.Id, cancellationToken);

        // 2. If not found, throw exception
        if (workoutPlan is null)
        {
            throw new NotFoundException("Workout Plan not found.");
        }

        // 3. Verify user has permission to delete this plan
        // Permission is granted if:
        // - User is the owner of the plan, OR
        // - User is the Personal Trainer of the plan's owner
        bool hasPermission = workoutPlan.OwnerId == request.OwnerId || // User is the owner
                           workoutPlan.Owner?.PersonalTrainerId == request.OwnerId; // User is the PT

        if (!hasPermission)
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