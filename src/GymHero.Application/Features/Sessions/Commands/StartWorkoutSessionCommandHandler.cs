using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Sessions.Commands;

public class StartWorkoutSessionCommandHandler : IRequestHandler<StartWorkoutSessionCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public StartWorkoutSessionCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(StartWorkoutSessionCommand request, CancellationToken cancellationToken)
    {
        // Validação: Se um plano foi fornecido, verificar se existe e pertence ao usuário
        if (request.WorkoutPlanId.HasValue)
        {
            var planExists = await _context.WorkoutPlans
                .AnyAsync(p => p.Id == request.WorkoutPlanId && p.OwnerId == request.OwnerId, cancellationToken);

            if (!planExists)
            {
                throw new NotFoundException("Workout Plan not found.");
            }
        }

        // Criamos a nova sessão de treino (com ou sem plano)
        var session = new WorkoutSession
        {
            OwnerId = request.OwnerId,
            WorkoutPlanId = request.WorkoutPlanId,
            StartedAt = DateTime.UtcNow // Registra a data e hora de início
        };

        await _context.WorkoutSessions.AddAsync(session, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return session.Id;
    }
}