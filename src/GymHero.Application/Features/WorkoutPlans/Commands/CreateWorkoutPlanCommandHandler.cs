using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using GymHero.Domain.Entities;
using MediatR;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public class CreateWorkoutPlanCommandHandler : IRequestHandler<CreateWorkoutPlanCommand, WorkoutPlanResponse>
{
    private readonly IApplicationDbContext _context;

    public CreateWorkoutPlanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WorkoutPlanResponse> Handle(CreateWorkoutPlanCommand request, CancellationToken cancellationToken)
    {
        // 1. Criar a entidade de domínio
        var workoutPlan = new WorkoutPlan
        {
            Name = request.Name,
            Goal = request.Goal,
            OwnerId = request.OwnerId // Associamos o plano ao usuário logado
        };

        // 2. Adicionar ao DbContext e salvar no banco
        await _context.WorkoutPlans.AddAsync(workoutPlan, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // 3. Mapear e retornar a resposta DTO
        return new WorkoutPlanResponse(
            workoutPlan.Id,
            workoutPlan.OwnerId,
            workoutPlan.Name,
            workoutPlan.Goal,
            workoutPlan.IsActive,
            workoutPlan.CreatedAt
        );
    }
}