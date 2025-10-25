using GymHero.Shared.DTOs;
using MediatR;
namespace GymHero.Application.Features.Friends.Queries;

public record GetFriendsQuery(Guid UserId) : IRequest<IEnumerable<FriendResponse>>;