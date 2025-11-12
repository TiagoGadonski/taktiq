using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using GymHero.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Friends.Queries;

public class GetPendingFriendRequestsQueryHandler : IRequestHandler<GetPendingFriendRequestsQuery, IEnumerable<FriendRequestResponse>>
{
    private readonly IApplicationDbContext _context;
    public GetPendingFriendRequestsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<FriendRequestResponse>> Handle(GetPendingFriendRequestsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Friendships
            .AsNoTracking()
            .Where(f => f.AddresseeId == request.UserId && f.Status == FriendshipStatus.Pending)
            .Include(f => f.Requester)
            .Select(f => new FriendRequestResponse(f.Id, f.RequesterId, f.Requester.Name, f.Requester.Email, f.Requester.ProfilePictureUrl))
            .ToListAsync(cancellationToken);
    }
}