using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Challenges.Queries;

public class GetAllChallengesQueryHandler : IRequestHandler<GetAllChallengesQuery, IEnumerable<ChallengeWithParticipationDto>>
{
    private readonly IApplicationDbContext _context;
    public GetAllChallengesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<ChallengeWithParticipationDto>> Handle(GetAllChallengesQuery request, CancellationToken cancellationToken)
    {
        var challenges = await _context.Challenges
            .AsNoTracking()
            .Include(c => c.Progresses)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return challenges.Select(c =>
        {
            var userProgress = c.Progresses.FirstOrDefault(p => p.ParticipantId == request.UserId);
            var isParticipating = userProgress != null;

            return new ChallengeWithParticipationDto
            {
                Id = c.Id,
                Title = c.Title,
                Type = c.Type,
                TargetValue = c.TargetValue,
                CurrentValue = userProgress?.CurrentValue ?? 0,
                Status = c.Status,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                TargetType = (ChallengeTargetType)c.TargetType,
                IsDefault = c.IsDefault,
                IsParticipating = isParticipating,
                Progresses = c.Progresses.Select(p => new ChallengeProgressDto
                {
                    ParticipantId = p.ParticipantId,
                    CurrentValue = p.CurrentValue,
                    LastUpdate = p.LastUpdate
                }).ToList()
            };
        });
    }
}
