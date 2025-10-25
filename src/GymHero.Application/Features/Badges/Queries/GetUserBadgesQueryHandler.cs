using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace GymHero.Application.Features.Badges.Queries;

public class GetUserBadgesQueryHandler : IRequestHandler<GetUserBadgesQuery, IEnumerable<BadgeResponse>>
{
    private readonly IApplicationDbContext _context;
    public GetUserBadgesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<BadgeResponse>> Handle(GetUserBadgesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Badges
            .AsNoTracking()
            .Where(b => b.OwnerId == request.UserId)
            .Select(b => new BadgeResponse(b.Title, b.Description, b.Code, b.EarnedAt))
            .ToListAsync(cancellationToken);
    }
}