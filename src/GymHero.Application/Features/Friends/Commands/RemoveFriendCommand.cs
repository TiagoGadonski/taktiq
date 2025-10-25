using MediatR;
namespace GymHero.Application.Features.Friends.Commands;

public record RemoveFriendCommand(Guid CurrentUserId, Guid FriendshipId) : IRequest;