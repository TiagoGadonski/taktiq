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
        // 1. Find the workout plan and verify user has permission
        var workoutPlan = await _context.WorkoutPlans
            .Include(wp => wp.Owner)
            .FirstOrDefaultAsync(wp => wp.Id == request.Id, cancellationToken);

        // 2. If not found, throw exception
        if (workoutPlan is null)
        {
            throw new NotFoundException("Workout Plan not found.");
        }

        // 3. Verify user has permission to modify this plan
        // Permission is granted if:
        // - User is the owner of the plan, OR
        // - User is the Personal Trainer of the plan's owner
        bool hasPermission = workoutPlan.OwnerId == request.OwnerId || // User is the owner
                           workoutPlan.Owner?.PersonalTrainerId == request.OwnerId; // User is the PT

        if (!hasPermission)
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