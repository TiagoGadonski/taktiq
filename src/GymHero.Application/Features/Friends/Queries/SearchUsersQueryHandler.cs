using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Enums;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Friends.Queries;

public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, IEnumerable<UserSearchResponse>>
{
    private readonly IApplicationDbContext _context;
    public SearchUsersQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<UserSearchResponse>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm) || request.SearchTerm.Length < 3)
        {
            return Enumerable.Empty<UserSearchResponse>();
        }

        var searchTermLower = request.SearchTerm.ToLower();

        var users = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id != request.CurrentUserId &&
                       (u.Name.ToLower().Contains(searchTermLower) || u.Email.ToLower().Contains(searchTermLower)))
            .Take(10)
            .ToListAsync(cancellationToken);

        // Get all friendships involving the current user
        var friendships = await _context.Friendships
            .AsNoTracking()
            .Where(f => f.RequesterId == request.CurrentUserId || f.AddresseeId == request.CurrentUserId)
            .ToListAsync(cancellationToken);

        return users.Select(u =>
        {
            var friendship = friendships.FirstOrDefault(f =>
                (f.RequesterId == request.CurrentUserId && f.AddresseeId == u.Id) ||
                (f.AddresseeId == request.CurrentUserId && f.RequesterId == u.Id));

            var isFriend = friendship?.Status == FriendshipStatus.Accepted;
            var hasPendingRequest = friendship?.Status == FriendshipStatus.Pending;

            return new UserSearchResponse(u.Id, u.Name, u.Email, isFriend, hasPendingRequest, u.ProfilePictureUrl);
        });
    }
}