using MediatR;
namespace GymHero.Application.Features.Friends.Commands;

public record SendFriendRequestByIdCommand(Guid RequesterId, Guid AddresseeId) : IRequest;