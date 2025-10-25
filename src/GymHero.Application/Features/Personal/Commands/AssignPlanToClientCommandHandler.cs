using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using GymHero.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Personal.Commands;

public class AssignPlanToClientCommandHandler : IRequestHandler<AssignPlanToClientCommand, WorkoutPlanResponse>
{
    private readonly IApplicationDbContext _context;
    public AssignPlanToClientCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<WorkoutPlanResponse> Handle(AssignPlanToClientCommand request, CancellationToken cancellationToken)
    {
        // Validação de Segurança Crítica: O aluno pertence a este personal?
        var client = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.ClientId && u.PersonalTrainerId == request.PersonalId, cancellationToken);
        
        if (client is null)
        {
            // Se não encontrar, ou o aluno não existe ou não pertence a este personal.
            // Por segurança, a mensagem é genérica.
            throw new NotFoundException("Client not found or not assigned to this trainer.");
        }

        // Se a validação passar, criamos o plano de treino
        var newPlan = new WorkoutPlan
        {
            Name = request.PlanName,
            Goal = request.PlanGoal,
            // O Dono (OwnerId) do plano é o ALUNO, não o personal.
            OwnerId = request.ClientId
        };
        
        await _context.WorkoutPlans.AddAsync(newPlan, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new WorkoutPlanResponse(newPlan.Id, newPlan.OwnerId, newPlan.Name, newPlan.Goal, newPlan.IsActive, newPlan.CreatedAt);
    }
}