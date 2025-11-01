using GymHero.Shared.DTOs;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Challenges.Commands;

public class CreateChallengeCommandHandler : IRequestHandler<CreateChallengeCommand, ChallengeResponse>
{
    private readonly IApplicationDbContext _context;
    public CreateChallengeCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<ChallengeResponse> Handle(CreateChallengeCommand request, CancellationToken cancellationToken)
    {
        var challenge = new Challenge
        {
            CreatorId = request.OwnerId,
            Title = request.Title,
            Type = request.Type,
            TargetValue = request.TargetValue,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "InProgress", // O desafio começa em progresso
            TargetType = (Domain.Enums.ChallengeTargetType)request.TargetType,
            IsDefault = request.IsDefault,
            IconName = request.IconName
        };

        // Ao criar um desafio, também criamos o seu registo de progresso inicial
        var progress = new ChallengeProgress
        {
            Challenge = challenge,
            ParticipantId = request.OwnerId,
            CurrentValue = 0,
            LastUpdate = DateTime.UtcNow
        };
        
        challenge.Progresses.Add(progress);

        await _context.Challenges.AddAsync(challenge, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Se for um desafio padrão, atribuir a todos os utilizadores existentes
        if (request.IsDefault)
        {
            await AssignToExistingUsers(challenge, cancellationToken);
        }

        return new ChallengeResponse(
            challenge.Id,
            challenge.Title,
            challenge.Type,
            challenge.TargetValue,
            challenge.Progresses.First().CurrentValue,
            challenge.Status,
            challenge.StartDate,
            challenge.EndDate,
            (ChallengeTargetType)challenge.TargetType,
            challenge.IsDefault,
            challenge.IconName
        );
    }

    private async Task AssignToExistingUsers(Challenge challenge, CancellationToken cancellationToken)
    {
        // Obter todos os utilizadores que devem receber este desafio
        var usersQuery = _context.Users.AsQueryable();

        if (challenge.TargetType == Domain.Enums.ChallengeTargetType.AllTrainers)
        {
            usersQuery = usersQuery.Where(u => u.Role == "Personal");
        }
        // AllUsers não precisa de filtro, SpecificUsers não deve chegar aqui (IsDefault=true)

        var users = await usersQuery.ToListAsync(cancellationToken);

        // Criar registos de progresso para cada utilizador
        foreach (var user in users)
        {
            // Evitar duplicar o progresso do criador (já foi adicionado)
            if (user.Id == challenge.CreatorId)
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