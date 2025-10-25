using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Challenges.Queries;

public class GetUserChallengesQueryHandler : IRequestHandler<GetUserChallengesQuery, IEnumerable<ChallengeResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetUserChallengesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<ChallengeResponse>> Handle(GetUserChallengesQuery request, CancellationToken cancellationToken)
    {
        var challenges = await _context.Challenges
            .AsNoTracking()
            // Incluímos a coleção de progressos
            .Include(c => c.Progresses) 
            // A condição de filtro muda: procuramos desafios onde o utilizador
            // tem um registo de progresso (ou seja, é um participante).
            .Where(c => c.Progresses.Any(p => p.ParticipantId == request.OwnerId))
            .OrderBy(c => c.StartDate)
            .Select(c => new ChallengeResponse(
                c.Id,
                c.Title,
                c.Type,
                c.TargetValue,
                // O progresso atual é a soma do progresso de todos os participantes
                // ou apenas o do utilizador, dependendo da regra do desafio.
                // Para desafios coletivos, somamos tudo.
                c.Progresses.Sum(p => p.CurrentValue),
                c.Status,
                c.StartDate,
                c.EndDate,
                (ChallengeTargetType)c.TargetType,
                c.IsDefault))
            .ToListAsync(cancellationToken);

        return challenges;
    }
}