using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Challenges.Commands;

public class CreateCustomChallengeCommandHandler : IRequestHandler<CreateCustomChallengeCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    public CreateCustomChallengeCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(CreateCustomChallengeCommand request, CancellationToken cancellationToken)
    {
        // 1. Criar a entidade principal do desafio
        var challenge = new Challenge
        {
            CreatorId = request.CreatorId,
            Title = request.Title,
            Type = request.Type,
            TargetValue = request.TargetValue,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "InProgress"
        };
        
        // 2. Adicionar o próprio criador como participante
        challenge.Progresses.Add(new ChallengeProgress
        {
            ParticipantId = request.CreatorId,
            CurrentValue = 0,
            LastUpdate = DateTime.UtcNow
        });

        // 3. Adicionar os amigos convidados como participantes
        foreach (var friendId in request.FriendIds)
        {
            // Numa aplicação real, validaríamos se estes IDs são realmente amigos do criador
            challenge.Progresses.Add(new ChallengeProgress
            {
                ParticipantId = friendId,
                CurrentValue = 0,
                LastUpdate = DateTime.UtcNow
            });
        }

        await _context.Challenges.AddAsync(challenge, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return challenge.Id;
    }
}