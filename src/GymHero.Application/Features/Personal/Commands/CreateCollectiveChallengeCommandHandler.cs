using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Personal.Commands;

public class CreateCollectiveChallengeCommandHandler : IRequestHandler<CreateCollectiveChallengeCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    public CreateCollectiveChallengeCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(CreateCollectiveChallengeCommand request, CancellationToken cancellationToken)
    {
        // 1. Criar a entidade principal do desafio.
        var challenge = new Challenge
        {
            CreatorId = request.PersonalId,
            Title = request.Title,
            Type = request.Type,
            TargetValue = request.TargetValue,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "InProgress"
        };
        
        // 2. Encontrar todos os clientes do personal.
        var clients = await _context.Users
            .Where(u => u.PersonalTrainerId == request.PersonalId)
            .ToListAsync(cancellationToken);

        // 3. Criar um registo de progresso para cada cliente.
        foreach (var client in clients)
        {
            challenge.Progresses.Add(new ChallengeProgress
            {
                ParticipantId = client.Id,
                CurrentValue = 0,
                LastUpdate = DateTime.UtcNow
            });
        }

        await _context.Challenges.AddAsync(challenge, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return challenge.Id;
    }
}