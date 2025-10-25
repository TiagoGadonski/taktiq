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
            Status = "InProgress",
            TargetType = (Domain.Enums.ChallengeTargetType)request.TargetType,
            IsDefault = request.IsDefault
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

        // Se for um desafio padrão, atribuir a todos os utilizadores existentes
        if (request.IsDefault)
        {
            await AssignToExistingUsers(challenge, request.FriendIds, cancellationToken);
        }

        return challenge.Id;
    }

    private async Task AssignToExistingUsers(Challenge challenge, List<Guid> excludeIds, CancellationToken cancellationToken)
    {
        // Obter todos os utilizadores que devem receber este desafio
        var usersQuery = _context.Users.AsQueryable();

        if (challenge.TargetType == Domain.Enums.ChallengeTargetType.AllTrainers)
        {
            usersQuery = usersQuery.Where(u => u.Role == "Personal");
        }
        // AllUsers não precisa de filtro

        var users = await usersQuery.ToListAsync(cancellationToken);

        // Criar registos de progresso para cada utilizador
        foreach (var user in users)
        {
            // Evitar duplicar progresso de utilizadores já adicionados (criador + amigos)
            if (user.Id == challenge.CreatorId || excludeIds.Contains(user.Id))
                continue;

            var progress = new ChallengeProgress
            {
                ChallengeId = challenge.Id,
                ParticipantId = user.Id,
                CurrentValue = 0,
                LastUpdate = DateTime.UtcNow
            };

            await _context.ChallengeProgresses.AddAsync(progress, cancellationToken);
        }

        if (users.Any())
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}