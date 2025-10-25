using MediatR;
namespace GymHero.Application.Features.Friends.Commands;

public record RespondToFriendRequestCommand(
    Guid CurrentUserId, // ID de quem está a responder (do token)
    Guid FriendshipId,
    bool Accept) : IRequest;