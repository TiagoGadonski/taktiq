using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Challenges.Queries;

public class GetMyChallengesQueryHandler : IRequestHandler<GetMyChallengesQuery, IEnumerable<ChallengeResponse>>
{
    private readonly IApplicationDbContext _context;
    public GetMyChallengesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<ChallengeResponse>> Handle(GetMyChallengesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Challenges
            .AsNoTracking()
            .Include(c => c.Progresses)
            .Where(c => c.Progresses.Any(p => p.ParticipantId == request.UserId)) // Filtra por desafios onde o user é participante
            .Select(c => new ChallengeResponse(
                c.Id,
                c.Title,
                c.Type,
                c.TargetValue,
                c.Progresses.Sum(p => p.CurrentValue), // Mostra o progresso total do desafio
                c.Status,
                c.StartDate,
                c.EndDate))
            .ToListAsync(cancellationToken);
    }
}