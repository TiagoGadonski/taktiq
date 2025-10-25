using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Friends.Commands;

public class RespondToFriendRequestCommandHandler : IRequestHandler<RespondToFriendRequestCommand>
{
    private readonly IApplicationDbContext _context;
    public RespondToFriendRequestCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(RespondToFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var friendship = await _context.Friendships
            .FirstOrDefaultAsync(f => f.Id == request.FriendshipId, cancellationToken);

        if (friendship is null)
            throw new NotFoundException("Pedido de amizade não encontrado.");

        // Validação de segurança: só o destinatário do pedido pode responder.
        if (friendship.AddresseeId != request.CurrentUserId)
            throw new ValidationException("Você não tem permissão para responder a este pedido.");

        if (friendship.Status != FriendshipStatus.Pending)
            throw new ValidationException("Este pedido já foi respondido.");

        friendship.Status = request.Accept ? FriendshipStatus.Accepted : FriendshipStatus.Declined;

        await _context.SaveChangesAsync(cancellationToken);
    }
}