using MediatR;
namespace GymHero.Application.Features.Friends.Commands;

public record SendFriendRequestCommand(
    Guid RequesterId, // ID de quem está a enviar (do token)
    string AddresseeEmail // Email de quem vai receber
) : IRequest;