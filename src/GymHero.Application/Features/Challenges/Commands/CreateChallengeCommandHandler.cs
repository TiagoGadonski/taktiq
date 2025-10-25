using GymHero.Shared.DTOs;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using MediatR;

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
            Status = "InProgress" // O desafio começa em progresso
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

        return new ChallengeResponse(
            challenge.Id,
            challenge.Title,
            challenge.Type,
            challenge.TargetValue,
            challenge.Progresses.First().CurrentValue,
            challenge.Status,
            challenge.StartDate,
            challenge.EndDate
        );
    }
}