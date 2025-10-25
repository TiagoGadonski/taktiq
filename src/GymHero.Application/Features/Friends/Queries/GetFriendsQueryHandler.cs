using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using GymHero.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Friends.Queries;

public class GetFriendsQueryHandler : IRequestHandler<GetFriendsQuery, IEnumerable<FriendResponse>>
{
    private readonly IApplicationDbContext _context;
    public GetFriendsQueryHandler(IApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<FriendResponse>> Handle(GetFriendsQuery request, CancellationToken cancellationToken)
    {
        var friends = await _context.Friendships
            .AsNoTracking()
            // 1. Encontrar todas as amizades ACEITES onde o nosso utilizador é ou o remetente ou o destinatário
            .Where(f => (f.RequesterId == request.UserId || f.AddresseeId == request.UserId) && f.Status == FriendshipStatus.Accepted)
            .Include(f => f.Requester)
            .Include(f => f.Addressee)
            // 2. Projetar o resultado para o nosso DTO
            .Select(f => new FriendResponse(
                f.Id,
                // Se o nosso utilizador é o remetente, o amigo é o destinatário. Se não, é o contrário.
                f.RequesterId == request.UserId ? f.AddresseeId : f.RequesterId,
                f.RequesterId == request.UserId ? f.Addressee.Name : f.Requester.Name
            ))
            .ToListAsync(cancellationToken);

        return friends;
    }
}